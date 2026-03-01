using System.ComponentModel.DataAnnotations;

namespace Endfield.Api.Models.InputDto.Tag;

/// <summary>
/// 创建标签请求
/// </summary>
public record CreateTagInputDto
{
    /// <summary>
    /// 标签名称
    /// </summary>
    [Required(ErrorMessage = "标签名称不能为空")]
    [MaxLength(50, ErrorMessage = "标签名称不能超过50个字符")]
    public required string Name { get; init; }

    /// <summary>
    /// 标签编码
    /// </summary>
    [Required(ErrorMessage = "标签编码不能为空")]
    [MaxLength(50, ErrorMessage = "标签编码不能超过50个字符")]
    public required string Code { get; init; }

    /// <summary>
    /// 标签描述
    /// </summary>
    [MaxLength(200, ErrorMessage = "标签描述不能超过200个字符")]
    public string? Description { get; init; }

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; init; }
}

/// <summary>
/// 更新标签请求
/// </summary>
public record UpdateTagInputDto
{
    /// <summary>
    /// 标签ID
    /// </summary>
    [Required(ErrorMessage = "标签ID不能为空")]
    public required int TagId { get; init; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [Required(ErrorMessage = "标签名称不能为空")]
    [MaxLength(50, ErrorMessage = "标签名称不能超过50个字符")]
    public required string Name { get; init; }

    /// <summary>
    /// 标签描述
    /// </summary>
    [MaxLength(200, ErrorMessage = "标签描述不能超过200个字符")]
    public string? Description { get; init; }

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; init; }
}

/// <summary>
/// 删除标签请求
/// </summary>
public record DeleteTagInputDto
{
    /// <summary>
    /// 标签ID
    /// </summary>
    [Required(ErrorMessage = "标签ID不能为空")]
    public required int TagId { get; init; }
}

/// <summary>
/// 查询标签请求
/// </summary>
public record QueryTagInputDto
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public int? TagId { get; init; }
}
