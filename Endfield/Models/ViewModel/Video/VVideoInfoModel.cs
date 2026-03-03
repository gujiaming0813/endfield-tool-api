using Endfield.Api.Models.ViewModel.Tag;

namespace Endfield.Api.Models.ViewModel.Video;

/// <summary>
/// 视频信息响应模型
/// </summary>
public record VVideoInfoModel
{
    /// <summary>
    /// 视频ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 视频BV号
    /// </summary>
    public required string Bvid { get; init; }

    /// <summary>
    /// 视频标题
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 视频封面URL
    /// </summary>
    public required string Cover { get; init; }

    /// <summary>
    /// 视频描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 视频时长（秒）
    /// </summary>
    public int Duration { get; init; }

    /// <summary>
    /// UP主昵称
    /// </summary>
    public required string OwnerName { get; init; }

    /// <summary>
    /// 视频跳转链接
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// 播放量
    /// </summary>
    public long ViewCount { get; init; }

    /// <summary>
    /// 点赞数
    /// </summary>
    public long LikeCount { get; init; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PublishTime { get; init; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; init; }

    /// <summary>
    /// 视频标签列表
    /// </summary>
    public List<VTagInfoModel> Tags { get; init; } = [];
}
