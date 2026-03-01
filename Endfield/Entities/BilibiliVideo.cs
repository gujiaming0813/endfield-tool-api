using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Endfield.Api.Entities;

/// <summary>
/// B站视频信息实体
/// </summary>
[Table("bilibili_videos")]
public class BilibiliVideo
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 视频BV号（唯一索引）
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("bvid")]
    public string Bvid { get; set; } = null!;

    /// <summary>
    /// 视频标题
    /// </summary>
    [Required]
    [MaxLength(500)]
    [Column("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 视频封面URL
    /// </summary>
    [Required]
    [MaxLength(1000)]
    [Column("cover")]
    public string Cover { get; set; } = null!;

    /// <summary>
    /// 视频描述
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// 视频时长（秒）
    /// </summary>
    [Column("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// UP主昵称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("owner_name")]
    public string OwnerName { get; set; } = null!;

    /// <summary>
    /// 视频跳转链接
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("url")]
    public string Url { get; set; } = null!;

    /// <summary>
    /// 播放量
    /// </summary>
    [Column("view_count")]
    public long ViewCount { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    [Column("like_count")]
    public long LikeCount { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [Column("publish_time")]
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 记录创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 记录更新时间
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已删除（软删除标记）
    /// </summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 视频标签关联（多对多）
    /// </summary>
    public virtual ICollection<VideoTagMapping> VideoTagMappings { get; set; } = [];
}
