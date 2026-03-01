namespace Endfield.Api.Models.ViewModel.Auth;

/// <summary>
/// 登录结果模型
/// </summary>
public record VLoginResultModel
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// 令牌类型
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public VUserInfoModel? User { get; init; }
}

/// <summary>
/// 用户信息模型
/// </summary>
public record VUserInfoModel
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Nickname { get; init; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; init; }
}
