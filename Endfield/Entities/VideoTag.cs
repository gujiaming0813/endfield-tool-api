namespace Endfield.Api.Entities;

/// <summary>
/// 视频标签实体
/// </summary>
public class VideoTag : BaseAuditModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 标签编码（用于程序标识）
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 标签描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 关联的视频（通过中间表）
    /// </summary>
    public virtual ICollection<VideoTagMapping> VideoTagMappings { get; set; } = [];
}
