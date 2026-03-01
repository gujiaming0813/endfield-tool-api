namespace Endfield.Api.Models.ViewModel.Tag;

/// <summary>
/// 标签信息响应模型
/// </summary>
public record VTagModel
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 标签名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 标签编码
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 标签描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 该标签下的视频数量
    /// </summary>
    public int VideoCount { get; init; }
}

/// <summary>
/// 简化的标签信息（用于视频响应中）
/// </summary>
public record VTagInfoModel
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 标签名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 标签编码
    /// </summary>
    public required string Code { get; init; }
}
