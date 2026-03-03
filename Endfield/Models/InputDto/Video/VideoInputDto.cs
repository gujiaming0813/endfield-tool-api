using System.ComponentModel.DataAnnotations;

namespace Endfield.Api.Models.InputDto.Video;

/// <summary>
/// 导入视频请求
/// </summary>
public record ImportVideoInputDto
{
    /// <summary>
    /// BV号或视频链接
    /// </summary>
    [Required(ErrorMessage = "请提供BV号或视频链接")]
    public required string Input { get; init; }

    /// <summary>
    /// 标签ID列表
    /// </summary>
    public List<int>? TagIds { get; init; }
}

/// <summary>
/// 更新视频请求
/// </summary>
public record UpdateVideoInputDto
{
    /// <summary>
    /// 视频ID
    /// </summary>
    [Required(ErrorMessage = "视频ID不能为空")]
    public required int VideoId { get; init; }

    /// <summary>
    /// 是否刷新视频信息（从B站API重新获取）
    /// </summary>
    public bool RefreshInfo { get; init; } = true;

    /// <summary>
    /// 是否置顶（为 null 则不更新置顶状态）
    /// </summary>
    public bool? IsPinned { get; init; }

    /// <summary>
    /// 标签ID列表（为空则不更新标签）
    /// </summary>
    public List<int>? TagIds { get; init; }
}

/// <summary>
/// 分页查询视频请求
/// </summary>
public record QueryVideoListInputDto
{
    /// <summary>
    /// 关键词（搜索标题、描述）
    /// </summary>
    public string? Keyword { get; init; }

    /// <summary>
    /// 标签ID列表（多个标签为AND关系）
    /// </summary>
    public List<int>? TagIds { get; init; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// 删除视频请求
/// </summary>
public record DeleteVideoInputDto
{
    /// <summary>
    /// 视频ID
    /// </summary>
    [Required(ErrorMessage = "视频ID不能为空")]
    public required int VideoId { get; init; }
}
