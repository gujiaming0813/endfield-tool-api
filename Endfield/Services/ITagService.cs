using Endfield.Api.Models.InputDto.Tag;
using Endfield.Api.Models.ViewModel.Tag;
using Endfield.Api.Share.IOCTag;
using Endfield.Api.Share.Models;

namespace Endfield.Api.Services;

/// <summary>
/// 标签服务接口
/// </summary>
public interface ITagService : IScopeTag
{
    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<ReturnDataModel<List<VTagModel>>> GetTagListAsync(CancellationToken token = default);

    /// <summary>
    /// 获取标签详情
    /// </summary>
    Task<ReturnDataModel<VTagModel>> GetTagByIdAsync(QueryTagInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 创建标签
    /// </summary>
    Task<ReturnDataModel<VTagModel>> CreateTagAsync(CreateTagInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 更新标签
    /// </summary>
    Task<ReturnDataModel<VTagModel>> UpdateTagAsync(UpdateTagInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    Task<ReturnDataModel<string>> DeleteTagAsync(DeleteTagInputDto inputDto, CancellationToken token = default);
}
