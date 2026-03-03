namespace Endfield.Api.Share.Enums;

/// <summary>
/// 视频刷新状态
/// </summary>
public enum VideoRefreshStatus
{
    /// <summary>
    /// 待刷新（初始状态）
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 刷新成功
    /// </summary>
    Success = 2,

    /// <summary>
    /// 刷新失败（需要重试）
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 限流中（等待重试）
    /// </summary>
    RateLimited = 4
}
