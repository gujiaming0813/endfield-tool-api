using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Models.InputDto.Auth;
using Endfield.Api.Models.ViewModel.Auth;
using Endfield.Api.Share.Enums;
using Endfield.Api.Share.Models;
using Endfield.Api.Share.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Endfield.Api.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService(
    AppDbContext dbContext,
    ITokenCacheService tokenCache,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<ReturnDataModel<VLoginResultModel>> LoginAsync(LoginInputDto inputDto, CancellationToken token = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == inputDto.Username, token);

        if (user == null)
        {
            return ReturnDataModel<VLoginResultModel>.FailResult("用户名或密码错误", ReturnDataCode.Unauthorized);
        }

        if (!user.IsActive)
        {
            return ReturnDataModel<VLoginResultModel>.FailResult("账号已被禁用", ReturnDataCode.Forbidden);
        }

        if (!VerifyPassword(inputDto.Password, user.Password))
        {
            return ReturnDataModel<VLoginResultModel>.FailResult("用户名或密码错误", ReturnDataCode.Unauthorized);
        }

        var accessToken = GenerateAccessToken(user);
        var expiresIn = _jwtOptions.ExpirationHours * 3600;

        // 将Token存入缓存（重新登录会使旧Token失效）
        await tokenCache.SetUserTokenAsync(
            user.Id,
            accessToken,
            TimeSpan.FromHours(_jwtOptions.ExpirationHours));

        var result = new VLoginResultModel
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            User = new VUserInfoModel
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Email = user.Email
            }
        };

        logger.LogInformation("用户登录成功: {Username}", user.Username);
        return ReturnDataModel<VLoginResultModel>.SuccessResult(result, "登录成功");
    }

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    public async Task<ReturnDataModel<VUserInfoModel>> GetCurrentUserAsync(int userId, CancellationToken token = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, token);

        if (user == null)
        {
            return ReturnDataModel<VUserInfoModel>.FailResult("用户不存在", ReturnDataCode.NotFound);
        }

        var result = new VUserInfoModel
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Email = user.Email
        };

        return ReturnDataModel<VUserInfoModel>.SuccessResult(result);
    }

    /// <summary>
    /// 注销登录
    /// </summary>
    public async Task<ReturnDataModel<string>> LogoutAsync(int userId)
    {
        await tokenCache.RemoveUserTokenAsync(userId);
        logger.LogInformation("用户注销登录: {UserId}", userId);
        return ReturnDataModel<string>.SuccessResult(userId.ToString(), "注销成功");
    }

    #region 私有方法

    /// <summary>
    /// 生成JWT访问令牌
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(user.Nickname))
        {
            claims.Add(new Claim("nickname", user.Nickname));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(_jwtOptions.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 密码加密（使用MD5）
    /// </summary>
    public static string HashPassword(string password)
    {
        var inputBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    private static bool VerifyPassword(string password, string hashedPassword)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hashedPassword;
    }

    #endregion
}
