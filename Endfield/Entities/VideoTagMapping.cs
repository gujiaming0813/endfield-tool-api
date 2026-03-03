namespace Endfield.Api.Entities;

/// <summary>
/// 视频标签关联表（多对多）
/// </summary>
public class VideoTagMapping
{
    /// <summary>
    /// 视频ID
    /// </summary>
    public int VideoId { get; set; }

    /// <summary>
    /// 标签ID
    /// </summary>
    public int TagId { get; set; }

    /// <summary>
    /// 关联创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 关联的视频
    /// </summary>
    public virtual BilibiliVideo Video { get; set; } = null!;

    /// <summary>
    /// 关联的标签
    /// </summary>
    public virtual VideoTag Tag { get; set; } = null!;
}
