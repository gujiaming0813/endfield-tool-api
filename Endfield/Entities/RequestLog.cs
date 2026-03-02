using Endfield.Api.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Endfield.Api.Entities;

/// <summary>
/// 请求日志实体
/// </summary>
public class RequestLog : BaseAuditModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 请求追踪ID
    /// </summary>
    [MaxLength(64)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestTime { get; set; }

    /// <summary>
    /// 请求方法 (GET, POST, PUT, DELETE等)
    /// </summary>
    [MaxLength(10)]
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    [MaxLength(500)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// 查询字符串
    /// </summary>
    [MaxLength(2000)]
    public string? QueryString { get; set; }

    /// <summary>
    /// 接口名称（Controller/Action）
    /// </summary>
    [MaxLength(200)]
    public string? ApiName { get; set; }

    /// <summary>
    /// 请求头（JSON格式）
    /// </summary>
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// 请求入参（JSON格式）
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    [MaxLength(50)]
    public string? ClientIp { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(100)]
    public string? UserName { get; set; }

    /// <summary>
    /// 响应时间
    /// </summary>
    public DateTime? ResponseTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 响应头（JSON格式）
    /// </summary>
    public string? ResponseHeaders { get; set; }

    /// <summary>
    /// 返回结果（JSON格式）
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    [MaxLength(200)]
    public string? ExceptionType { get; set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    [MaxLength(4000)]
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    public string? ExceptionStackTrace { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    [MaxLength(20)]
    public string? LogLevel { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    [MaxLength(4000)]
    public string? Message { get; set; }

    /// <summary>
    /// 额外数据（JSON格式，用于存储其他自定义字段）
    /// </summary>
    public string? ExtraData { get; set; }

    /// <summary>
    /// 运行环境
    /// </summary>
    [MaxLength(50)]
    public string? Environment { get; set; }

    /// <summary>
    /// 机器名
    /// </summary>
    [MaxLength(100)]
    public string? MachineName { get; set; }
}

/// <summary>
/// 请求日志静态扩展方法
/// </summary>
public static class RequestLogExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支持中文不转义
    };

    /// <summary>
    /// 序列化对象为JSON字符串
    /// </summary>
    public static string? ToJsonString(this object? obj)
    {
        if (obj == null) return null;
        try
        {
            return JsonSerializer.Serialize(obj, JsonOptions);
        }
        catch
        {
            return obj.ToString();
        }
    }
}
