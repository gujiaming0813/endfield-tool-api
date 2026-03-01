using Endfield.Api.Models.InputDto.Auth;
using Endfield.Api.Models.ViewModel.Auth;
using Endfield.Api.Share.IOCTag;
using Endfield.Api.Share.Models;

namespace Endfield.Api.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService : IScopeTag
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<ReturnDataModel<VLoginResultModel>> LoginAsync(LoginInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    Task<ReturnDataModel<VUserInfoModel>> GetCurrentUserAsync(int userId, CancellationToken token = default);

    /// <summary>
    /// 注销登录
    /// </summary>
    Task<ReturnDataModel<string>> LogoutAsync(int userId);
}
