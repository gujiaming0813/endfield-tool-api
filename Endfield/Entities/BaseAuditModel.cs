namespace Endfield.Api.Entities;

/// <summary>
/// 审计模型基类
/// </summary>
public abstract class BaseAuditModel
{
    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 是否删除（软删除标记）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 创建人ID
    /// </summary>
    public long? CreatedBy { get; set; }

    /// <summary>
    /// 创建人名称
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后修改人ID
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 最后修改人名称
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
