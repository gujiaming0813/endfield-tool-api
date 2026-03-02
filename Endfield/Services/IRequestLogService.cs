using Endfield.Api.Entities;

namespace Endfield.Api.Services;

/// <summary>
/// 请求日志服务接口
/// </summary>
public interface IRequestLogService
{
    /// <summary>
    /// 异步保存请求日志到数据库（不阻塞请求）
    /// </summary>
    /// <param name="requestLog">请求日志实体</param>
    /// <returns>任务</returns>
    Task SaveLogAsync(RequestLog requestLog);

    /// <summary>
    /// 创建请求日志实体（不保存）
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="requestBody">请求体</param>
    /// <returns>请求日志实体</returns>
    RequestLog CreateRequestLog(HttpContext httpContext, string? requestBody = null);
}
