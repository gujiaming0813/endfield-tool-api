using Endfield.Api.Share.IOCTag;

namespace Endfield.Api.Services;

/// <summary>
/// Token缓存服务接口
/// </summary>
public interface ITokenCacheService : ISingletonTag
{
    /// <summary>
    /// 设置用户Token
    /// </summary>
    Task SetUserTokenAsync(int userId, string token, TimeSpan expiration);

    /// <summary>
    /// 获取用户Token
    /// </summary>
    Task<string?> GetUserTokenAsync(int userId);

    /// <summary>
    /// 移除用户Token（注销登录）
    /// </summary>
    Task RemoveUserTokenAsync(int userId);

    /// <summary>
    /// 验证Token是否有效
    /// </summary>
    Task<bool> ValidateTokenAsync(int userId, string token);
}
