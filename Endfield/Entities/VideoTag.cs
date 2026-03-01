using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Endfield.Api.Entities;

/// <summary>
/// 视频标签实体
/// </summary>
[Table("video_tags")]
public class VideoTag
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 标签编码（用于程序标识）
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 标签描述
    /// </summary>
    [MaxLength(200)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序权重
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 记录创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已删除（软删除标记）
    /// </summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 关联的视频（通过中间表）
    /// </summary>
    public virtual ICollection<VideoTagMapping> VideoTagMappings { get; set; } = [];
}
