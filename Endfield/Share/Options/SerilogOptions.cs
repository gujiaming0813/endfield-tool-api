namespace Endfield.Api.Share.Options;

/// <summary>
/// Serilog配置选项
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string ApplicationName { get; set; } = "EndfieldApi";

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// 控制台输出配置
    /// </summary>
    public ConsoleOptions Console { get; set; } = new();

    /// <summary>
    /// 文件输出配置
    /// </summary>
    public FileOptions File { get; set; } = new();

    /// <summary>
    /// 是否输出JSON格式（适合日志收集系统如阿里云SLS）
    /// </summary>
    public bool OutputJsonFormat { get; set; } = true;

    /// <summary>
    /// 阿里云日志服务配置
    /// </summary>
    public AliyunSlsOptions? AliyunSls { get; set; }
}

/// <summary>
/// 控制台输出配置
/// </summary>
public class ConsoleOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 输出模板（非JSON格式时使用）
    /// </summary>
    public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
}

/// <summary>
/// 文件输出配置
/// </summary>
public class FileOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string Path { get; set; } = "logs/log-.json";

    /// <summary>
    /// 滚动间隔
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// 保留天数
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// 单个文件最大大小（MB）
    /// </summary>
    public long FileSizeLimitMb { get; set; } = 100;

    /// <summary>
    /// 输出模板（非JSON格式时使用）
    /// </summary>
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}

/// <summary>
/// 阿里云日志服务配置
/// </summary>
public class AliyunSlsOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 阿里云AccessKeyId
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// 阿里云AccessKeySecret
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// 日志服务Endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Project名称
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Logstore名称
    /// </summary>
    public string Logstore { get; set; } = string.Empty;
}
