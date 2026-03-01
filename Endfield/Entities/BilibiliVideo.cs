namespace Endfield.Api.Entities;

/// <summary>
/// B站视频信息实体
/// </summary>
public class BilibiliVideo : BaseAuditModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 视频BV号（唯一索引）
    /// </summary>
    public string Bvid { get; set; } = null!;

    /// <summary>
    /// 视频标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 视频封面URL
    /// </summary>
    public string Cover { get; set; } = null!;

    /// <summary>
    /// 视频描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 视频时长（秒）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// UP主昵称
    /// </summary>
    public string OwnerName { get; set; } = null!;

    /// <summary>
    /// 视频跳转链接
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// 播放量
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    public long LikeCount { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 视频标签关联（多对多）
    /// </summary>
    public virtual ICollection<VideoTagMapping> VideoTagMappings { get; set; } = [];
}
