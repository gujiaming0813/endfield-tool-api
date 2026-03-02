using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Serilog.Events;

namespace Endfield.Api.Filters;

/// <summary>
/// 全局日志拦截器 - AOP方式记录请求和响应
/// </summary>
public class LogActionFilter : IAsyncActionFilter
{
    private const string StopwatchKey = "__LogActionFilter_Stopwatch__";
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "pwd", "secret", "token", "accesstoken", "refreshtoken",
        "apikey", "api_key", "authorization", "creditcard", "cardnumber"
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var httpRequest = context.HttpContext.Request;
        var correlationId = Guid.NewGuid().ToString("N");

        // 存储到 HttpContext 供后续使用
        context.HttpContext.Items["CorrelationId"] = correlationId;
        context.HttpContext.Items[StopwatchKey] = stopwatch;

        // 获取请求信息
        var requestInfo = new
        {
            CorrelationId = correlationId,
            Method = httpRequest.Method,
            Path = httpRequest.Path.Value,
            QueryString = httpRequest.QueryString.Value,
            UserAgent = httpRequest.Headers.UserAgent.ToString(),
            IpAddress = GetClientIpAddress(context.HttpContext),
            UserId = context.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Controller = context.Controller?.GetType().Name,
            Action = context.ActionDescriptor.RouteValues["action"],
            Arguments = SanitizeArguments(context.ActionArguments)
        };

        // 记录请求开始
        Log.Information("HTTP请求开始 {@RequestInfo}", requestInfo);

        // 执行Action
        var executedContext = await next();

        stopwatch.Stop();

        // 记录响应
        var responseInfo = new
        {
            CorrelationId = correlationId,
            StatusCode = executedContext.HttpContext.Response.StatusCode,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            Success = executedContext.Exception == null,
            Exception = executedContext.Exception != null ? new
            {
                Type = executedContext.Exception.GetType().Name,
                Message = executedContext.Exception.Message,
                StackTrace = executedContext.Exception.StackTrace
            } : null
        };

        if (executedContext.Exception != null)
        {
            Log.Error(executedContext.Exception, "HTTP请求异常 {@ResponseInfo}", responseInfo);
        }
        else
        {
            Log.Information("HTTP请求完成 {@ResponseInfo}", responseInfo);
        }
    }

    /// <summary>
    /// 获取客户端IP地址
    /// </summary>
    private static string GetClientIpAddress(HttpContext httpContext)
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            ip = ip.Split(',').FirstOrDefault()?.Trim();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = httpContext.Connection.RemoteIpAddress?.ToString();
        }

        return ip ?? "unknown";
    }

    /// <summary>
    /// 清理敏感参数
    /// </summary>
    private static Dictionary<string, object?> SanitizeArguments(IDictionary<string, object?>? arguments)
    {
        if (arguments == null || arguments.Count == 0)
        {
            return new Dictionary<string, object?>();
        }

        var sanitized = new Dictionary<string, object?>();
        foreach (var arg in arguments)
        {
            if (IsSensitiveField(arg.Key))
            {
                sanitized[arg.Key] = "******";
            }
            else if (arg.Value == null)
            {
                sanitized[arg.Key] = null;
            }
            else if (arg.Value.GetType().IsValueType || arg.Value is string)
            {
                sanitized[arg.Key] = arg.Value;
            }
            else
            {
                // 复杂对象，序列化并清理敏感字段
                try
                {
                    var json = JsonSerializer.Serialize(arg.Value);
                    sanitized[arg.Key] = SanitizeJson(json);
                }
                catch
                {
                    sanitized[arg.Key] = "[无法序列化]";
                }
            }
        }
        return sanitized;
    }

    /// <summary>
    /// 检查是否是敏感字段
    /// </summary>
    private static bool IsSensitiveField(string fieldName)
    {
        return SensitiveFields.Contains(fieldName);
    }

    /// <summary>
    /// 清理JSON中的敏感字段
    /// </summary>
    private static string SanitizeJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                SanitizeJsonElement(doc.RootElement, jsonWriter);
                jsonWriter.Flush();
            }
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch
        {
            return json;
        }
    }

    private static void SanitizeJsonElement(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    if (IsSensitiveField(property.Name))
                    {
                        writer.WriteString(property.Name, "******");
                    }
                    else
                    {
                        writer.WritePropertyName(property.Name);
                        SanitizeJsonElement(property.Value, writer);
                    }
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    SanitizeJsonElement(item, writer);
                }
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                    writer.WriteNumberValue(longValue);
                else
                    writer.WriteNumberValue(element.GetDouble());
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
        }
    }
}

/// <summary>
/// 全局异常过滤器 - 捕获未处理异常
/// </summary>
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N");

        Log.Error(context.Exception, "全局异常捕获 {@ExceptionInfo}", new
        {
            CorrelationId = correlationId,
            Path = context.HttpContext.Request.Path.Value,
            Method = context.HttpContext.Request.Method,
            ExceptionType = context.Exception.GetType().Name,
            ExceptionMessage = context.Exception.Message,
            StackTrace = context.Exception.StackTrace
        });

        return Task.CompletedTask;
    }
}
