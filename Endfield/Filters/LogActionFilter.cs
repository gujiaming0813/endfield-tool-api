using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Endfield.Api.Entities;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Endfield.Api.Filters;

/// <summary>
/// 全局日志拦截器 - AOP方式记录请求和响应，并保存到数据库
/// </summary>
public class LogActionFilter : IAsyncActionFilter
{
    private const string RequestLogKey = "__LogActionFilter_RequestLog__";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "pwd", "secret", "token", "accesstoken", "refreshtoken",
        "apikey", "api_key", "authorization", "creditcard", "cardnumber"
    };

    private readonly IRequestLogService _requestLogService;

    public LogActionFilter(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var httpRequest = context.HttpContext.Request;
        var correlationId = Guid.NewGuid().ToString("N");

        // 存储到 HttpContext 供后续使用
        context.HttpContext.Items["CorrelationId"] = correlationId;

        // 存储 API 名称（Controller/Action）
        var controller = context.ActionDescriptor.RouteValues["controller"] ?? "";
        var action = context.ActionDescriptor.RouteValues["action"] ?? "";
        var apiName = $"{controller}/{action}";
        context.HttpContext.Items["ApiName"] = apiName;

        // 读取请求体（需要启用缓冲）
        string? requestBody = null;
        if (httpRequest.ContentLength > 0)
        {
            try
            {
                httpRequest.EnableBuffering();
                using var reader = new StreamReader(httpRequest.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                httpRequest.Body.Position = 0;
                requestBody = SanitizeJson(requestBody);
            }
            catch
            {
                requestBody = "[无法读取请求体]";
            }
        }

        // 获取 Action 参数作为请求体的备选
        if (string.IsNullOrEmpty(requestBody) && context.ActionArguments.Count > 0)
        {
            requestBody = JsonSerializer.Serialize(SanitizeArguments(context.ActionArguments), JsonOptions);
        }

        // 创建请求日志实体
        var requestLog = _requestLogService.CreateRequestLog(context.HttpContext, requestBody);
        context.HttpContext.Items[RequestLogKey] = requestLog;

        // 执行Action
        var executedContext = await next();

        stopwatch.Stop();

        // 获取响应体
        string? responseBody = null;
        if (executedContext.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            try
            {
                responseBody = JsonSerializer.Serialize(objectResult.Value, JsonOptions);
            }
            catch
            {
                responseBody = "[无法序列化响应]";
            }
        }
        else if (executedContext.Result is Microsoft.AspNetCore.Mvc.JsonResult jsonResult)
        {
            try
            {
                responseBody = JsonSerializer.Serialize(jsonResult.Value, JsonOptions);
            }
            catch
            {
                responseBody = "[无法序列化响应]";
            }
        }

        // 更新响应信息到请求日志
        RequestLogService.UpdateResponseInfo(requestLog, context.HttpContext, responseBody, executedContext.Exception);

        // 异步保存日志到数据库（不阻塞请求）
        await _requestLogService.SaveLogAsync(requestLog);

        // 输出简化的请求日志（合并为一条）
        var method = httpRequest.Method;
        var path = httpRequest.Path.Value ?? "";
        var statusCode = executedContext.Exception != null ? 500 : requestLog.StatusCode;
        var duration = stopwatch.ElapsedMilliseconds;
        var shortId = correlationId[..8]; // 日志显示时使用短码，便于阅读

        if (executedContext.Exception != null)
        {
            Log.Error("[{ShortId}] {Method} {Path} -> {StatusCode} ({Duration}ms) | 异常: {ExceptionMessage}",
                shortId, method, path, statusCode, duration, executedContext.Exception.Message);
        }
        else
        {
            Log.Information("[{ShortId}] {Method} {Path} -> {StatusCode} ({Duration}ms)",
                shortId, method, path, statusCode, duration);
        }
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
                try
                {
                    var json = JsonSerializer.Serialize(arg.Value, JsonOptions);
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
            using (var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
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
        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "unknown";
        var shortId = correlationId.Length >= 8 ? correlationId[..8] : correlationId;
        var request = context.HttpContext.Request;

        Log.Error("[{ShortId}] 全局异常 | {Method} {Path} | {ExceptionType}: {ExceptionMessage}",
            shortId,
            request.Method,
            request.Path.Value,
            context.Exception.GetType().Name,
            context.Exception.Message);

        return Task.CompletedTask;
    }
}
