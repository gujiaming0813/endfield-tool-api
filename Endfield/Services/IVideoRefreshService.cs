using Endfield.Api.Share.IOCTag;

namespace Endfield.Api.Services;

/// <summary>
/// 视频刷新服务接口
/// </summary>
public interface IVideoRefreshService : IScopeTag
{
    /// <summary>
    /// 刷新近一个月发布的视频信息
    /// </summary>
    Task RefreshRecentVideosAsync(CancellationToken token = default);
}
