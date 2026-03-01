namespace Endfield.Api.Share.Options;

/// <summary>
/// JWT配置选项
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// 签发者
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// 过期时间（小时）
    /// </summary>
    public int ExpirationHours { get; set; } = 24;
}
