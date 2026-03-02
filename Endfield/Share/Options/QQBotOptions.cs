namespace Endfield.Api.Share.Options;

/// <summary>
/// QQ机器人配置选项
/// </summary>
public class QQBotOptions
{
    /// <summary>
    /// 机器人AppID
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// 机器人Token（Bot Token）
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// 机器人AppSecret（用于签名验证）
    /// </summary>
    public string AppSecret { get; set; } = null!;

    /// <summary>
    /// API基础地址
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.sgroup.qq.com";

    /// <summary>
    /// 沙箱模式API地址
    /// </summary>
    public string SandboxApiBaseUrl { get; set; } = "https://sandbox.api.sgroup.qq.com";

    /// <summary>
    /// 是否启用沙箱模式
    /// </summary>
    public bool UseSandbox { get; set; } = true;

    /// <summary>
    /// 是否启用机器人
    /// </summary>
    public bool Enabled { get; set; } = true;
}
