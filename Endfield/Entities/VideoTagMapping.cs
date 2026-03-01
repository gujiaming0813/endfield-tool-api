using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Endfield.Api.Entities;

/// <summary>
/// 视频标签关联表（多对多）
/// </summary>
[Table("video_tag_mappings")]
public class VideoTagMapping
{
    /// <summary>
    /// 视频ID
    /// </summary>
    [Key]
    [Column("video_id", Order = 0)]
    public int VideoId { get; set; }

    /// <summary>
    /// 标签ID
    /// </summary>
    [Key]
    [Column("tag_id", Order = 1)]
    public int TagId { get; set; }

    /// <summary>
    /// 关联创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 关联的视频
    /// </summary>
    [ForeignKey(nameof(VideoId))]
    public virtual BilibiliVideo Video { get; set; } = null!;

    /// <summary>
    /// 关联的标签
    /// </summary>
    [ForeignKey(nameof(TagId))]
    public virtual VideoTag Tag { get; set; } = null!;
}
