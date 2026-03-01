using System.Security.Claims;
using Endfield.Api.Models.InputDto.Auth;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Endfield.Api.Controllers;

/// <summary>
/// 认证接口
/// </summary>
[Authorize]
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> LoginAsync(LoginInputDto inputDto, CancellationToken token)
    {
        var res = await authService.LoginAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 注销登录
    /// </summary>
    [HttpPost("Logout")]
    public async Task<ActionResult<string>> LogoutAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return ReturnFail("未授权访问", Share.Enums.ReturnDataCode.Unauthorized);
        }

        var res = await authService.LogoutAsync(userId);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpPost("GetCurrentUser")]
    public async Task<ActionResult<string>> GetCurrentUserAsync(CancellationToken token)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return ReturnFail("未授权访问", Share.Enums.ReturnDataCode.Unauthorized);
        }

        var res = await authService.GetCurrentUserAsync(userId, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }
}
