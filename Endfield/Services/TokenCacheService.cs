using Microsoft.Extensions.Caching.Memory;

namespace Endfield.Api.Services;

/// <summary>
/// Token缓存服务实现（基于内存缓存）
/// </summary>
public class TokenCacheService(IMemoryCache cache) : ITokenCacheService
{
    private static string GetTokenKey(int userId) => $"user_token_{userId}";

    /// <summary>
    /// 设置用户Token
    /// </summary>
    public Task SetUserTokenAsync(int userId, string token, TimeSpan expiration)
    {
        var key = GetTokenKey(userId);
        cache.Set(key, token, expiration);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取用户Token
    /// </summary>
    public Task<string?> GetUserTokenAsync(int userId)
    {
        var key = GetTokenKey(userId);
        cache.TryGetValue(key, out string? token);
        return Task.FromResult(token);
    }

    /// <summary>
    /// 移除用户Token（注销登录）
    /// </summary>
    public Task RemoveUserTokenAsync(int userId)
    {
        var key = GetTokenKey(userId);
        cache.Remove(key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 验证Token是否有效
    /// </summary>
    public async Task<bool> ValidateTokenAsync(int userId, string token)
    {
        var cachedToken = await GetUserTokenAsync(userId);
        return cachedToken == token;
    }
}
