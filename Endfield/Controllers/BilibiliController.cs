using Endfield.Api.Models.InputDto.Video;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Endfield.Api.Controllers;

/// <summary>
/// B站视频接口
/// </summary>
[Authorize]
public class BilibiliController(IBilibiliService service) : BaseController
{
    /// <summary>
    /// 导入视频（通过链接，可同时设置标签）
    /// </summary>
    [HttpPost("ImportVideo")]
    public async Task<ActionResult<string>> ImportVideoAsync(ImportVideoInputDto inputDto, CancellationToken token)
    {
        var res = await service.ImportVideoAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 更新视频（刷新信息并更新标签）
    /// </summary>
    [HttpPost("UpdateVideo")]
    public async Task<ActionResult<string>> UpdateVideoAsync(UpdateVideoInputDto inputDto, CancellationToken token)
    {
        var res = await service.UpdateVideoAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 分页查询视频列表（支持关键词和标签筛选）
    /// </summary>
    [HttpPost("QueryVideoList")]
    public async Task<ActionResult<string>> QueryVideoListAsync(QueryVideoListInputDto inputDto, CancellationToken token)
    {
        var res = await service.QueryVideoListAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 获取视频详情
    /// </summary>
    [HttpPost("GetVideoById")]
    public async Task<ActionResult<string>> GetVideoByIdAsync([FromQuery] int videoId, CancellationToken token)
    {
        var res = await service.GetVideoByIdAsync(videoId, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 删除视频（软删除）
    /// </summary>
    [HttpPost("DeleteVideo")]
    public async Task<ActionResult<string>> DeleteVideoAsync(DeleteVideoInputDto inputDto, CancellationToken token)
    {
        var res = await service.DeleteVideoAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }
}
