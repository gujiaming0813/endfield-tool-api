using Endfield.Api.Models.InputDto.Tag;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Endfield.Api.Controllers;

/// <summary>
/// 标签管理接口
/// </summary>
[Authorize]
public class TagsController(ITagService service) : BaseController
{
    /// <summary>
    /// 获取所有标签
    /// </summary>
    [HttpPost("GetTagList")]
    public async Task<ActionResult<string>> GetTagListAsync(CancellationToken token)
    {
        var res = await service.GetTagListAsync(token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 获取标签详情
    /// </summary>
    [HttpPost("GetTagById")]
    public async Task<ActionResult<string>> GetTagByIdAsync(QueryTagInputDto inputDto, CancellationToken token)
    {
        var res = await service.GetTagByIdAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 创建标签
    /// </summary>
    [HttpPost("CreateTag")]
    public async Task<ActionResult<string>> CreateTagAsync(CreateTagInputDto inputDto, CancellationToken token)
    {
        var res = await service.CreateTagAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    [HttpPost("UpdateTag")]
    public async Task<ActionResult<string>> UpdateTagAsync(UpdateTagInputDto inputDto, CancellationToken token)
    {
        var res = await service.UpdateTagAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    [HttpPost("DeleteTag")]
    public async Task<ActionResult<string>> DeleteTagAsync(DeleteTagInputDto inputDto, CancellationToken token)
    {
        var res = await service.DeleteTagAsync(inputDto, token);
        return ReturnInfo(res.Success, res.Code, res.Message, res.Data);
    }
}
