namespace Endfield.Api.Models.ViewModel.Common;

/// <summary>
/// 分页响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public record VBasePagingViewModel<T>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Data { get; init; } = [];
}
