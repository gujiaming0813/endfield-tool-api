using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Share.IOCTag;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Services;

/// <summary>
/// 请求日志服务实现
/// </summary>
public class RequestLogService : IRequestLogService, ITransientTag
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RequestLogService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支持中文不转义
    };

    public RequestLogService(IServiceScopeFactory scopeFactory, ILogger<RequestLogService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 异步保存请求日志到数据库（Fire-and-Forget模式，不阻塞请求）
    /// </summary>
    public Task SaveLogAsync(RequestLog requestLog)
    {
        // 使用 Fire-and-Forget 模式，不阻塞请求
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.RequestLogs.Add(requestLog);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存请求日志到数据库失败，CorrelationId: {CorrelationId}", requestLog.CorrelationId);
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建请求日志实体（不保存）
    /// </summary>
    public RequestLog CreateRequestLog(HttpContext httpContext, string? requestBody = null)
    {
        var request = httpContext.Request;
        var user = httpContext.User;

        // 获取用户信息
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userNameClaim = user?.FindFirst(ClaimTypes.Name)?.Value;
        _ = int.TryParse(userIdClaim, out var userId);

        // 获取API名称（Controller/Action）- 从 HttpContext.Items 获取（由 LogActionFilter 设置）
        var apiName = httpContext.Items["ApiName"]?.ToString();

        // 序列化请求头
        var requestHeaders = SerializeHeaders(request.Headers);

        var requestLog = new RequestLog
        {
            CorrelationId = httpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N"),
            RequestTime = DateTime.UtcNow,
            RequestMethod = request.Method,
            RequestPath = request.Path.Value,
            QueryString = request.QueryString.Value,
            ApiName = apiName,
            RequestHeaders = requestHeaders,
            RequestBody = TruncateString(requestBody, 50000), // 限制大小
            ClientIp = GetClientIpAddress(httpContext),
            UserAgent = request.Headers.UserAgent.ToString(),
            UserId = userId > 0 ? userId : null,
            UserName = userNameClaim,
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName = System.Environment.MachineName
        };

        return requestLog;
    }

    /// <summary>
    /// 更新响应信息到请求日志
    /// </summary>
    public static void UpdateResponseInfo(RequestLog requestLog, HttpContext httpContext, string? responseBody, Exception? exception = null)
    {
        var response = httpContext.Response;

        requestLog.ResponseTime = DateTime.UtcNow;
        requestLog.DurationMs = (long)(requestLog.ResponseTime.Value - requestLog.RequestTime).TotalMilliseconds;
        requestLog.StatusCode = response.StatusCode;
        requestLog.IsSuccess = exception == null && response.StatusCode is >= 200 and < 400;
        requestLog.ResponseHeaders = SerializeHeaders(response.Headers);
        requestLog.ResponseBody = TruncateString(responseBody, 50000); // 限制大小
        requestLog.LogLevel = exception == null ? "Information" : "Error";

        if (exception != null)
        {
            requestLog.ExceptionType = exception.GetType().Name;
            requestLog.ExceptionMessage = TruncateString(exception.Message, 4000);
            requestLog.ExceptionStackTrace = TruncateString(exception.StackTrace, 10000);
            requestLog.Message = $"请求异常: {exception.Message}";
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
    /// 序列化请求头/响应头
    /// </summary>
    private static string? SerializeHeaders(IHeaderDictionary headers)
    {
        try
        {
            var headerDict = headers
                .Where(h => !string.IsNullOrEmpty(h.Key))
                .ToDictionary(
                    h => h.Key,
                    h => h.Value.ToString()
                );
            return JsonSerializer.Serialize(headerDict, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 截断字符串到指定长度
    /// </summary>
    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length > maxLength ? value[..maxLength] + "...[truncated]" : value;
    }
}
