using Endfield.Api.Models.InputDto.Video;
using Endfield.Api.Models.ViewModel.Common;
using Endfield.Api.Models.ViewModel.Video;
using Endfield.Api.Share.IOCTag;
using Endfield.Api.Share.Models;

namespace Endfield.Api.Services;

/// <summary>
/// B站视频服务接口
/// </summary>
public interface IBilibiliService : IScopeTag
{
    /// <summary>
    /// 导入视频（通过链接，可同时设置标签）
    /// </summary>
    Task<ReturnDataModel<VVideoInfoModel>> ImportVideoAsync(ImportVideoInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 更新视频（刷新信息并更新标签）
    /// </summary>
    Task<ReturnDataModel<VVideoInfoModel>> UpdateVideoAsync(UpdateVideoInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 分页查询视频列表（支持关键词和标签筛选）
    /// </summary>
    Task<ReturnDataModel<VBasePagingViewModel<VVideoInfoModel>>> QueryVideoListAsync(QueryVideoListInputDto inputDto, CancellationToken token = default);

    /// <summary>
    /// 获取视频详情
    /// </summary>
    Task<ReturnDataModel<VVideoInfoModel>> GetVideoByIdAsync(int videoId, CancellationToken token = default);

    /// <summary>
    /// 删除视频（软删除）
    /// </summary>
    Task<ReturnDataModel<string>> DeleteVideoAsync(DeleteVideoInputDto inputDto, CancellationToken token = default);
}
