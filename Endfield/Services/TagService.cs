using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Models.InputDto.Tag;
using Endfield.Api.Models.ViewModel.Tag;
using Endfield.Api.Share.Enums;
using Endfield.Api.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Services;

/// <summary>
/// 标签服务实现
/// </summary>
public class TagService(AppDbContext dbContext, ILogger<TagService> logger) : ITagService
{
    /// <summary>
    /// 获取所有标签
    /// </summary>
    public async Task<ReturnDataModel<List<VTagModel>>> GetTagListAsync(CancellationToken token = default)
    {
        var tags = await dbContext.VideoTags
            .OrderBy(t => t.SortOrder)
            .Select(t => new VTagModel
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                VideoCount = t.VideoTagMappings.Count
            })
            .ToListAsync(token);

        return ReturnDataModel<List<VTagModel>>.SuccessResult(tags);
    }

    /// <summary>
    /// 获取标签详情
    /// </summary>
    public async Task<ReturnDataModel<VTagModel>> GetTagByIdAsync(QueryTagInputDto inputDto, CancellationToken token = default)
    {
        if (inputDto.TagId == null)
        {
            return ReturnDataModel<VTagModel>.FailResult("标签ID不能为空", ReturnDataCode.BadRequest);
        }

        var tag = await dbContext.VideoTags
            .Where(t => t.Id == inputDto.TagId)
            .Select(t => new VTagModel
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                VideoCount = t.VideoTagMappings.Count
            })
            .FirstOrDefaultAsync(token);

        if (tag == null)
        {
            return ReturnDataModel<VTagModel>.FailResult("标签不存在", ReturnDataCode.NotFound);
        }

        return ReturnDataModel<VTagModel>.SuccessResult(tag);
    }

    /// <summary>
    /// 创建标签
    /// </summary>
    public async Task<ReturnDataModel<VTagModel>> CreateTagAsync(CreateTagInputDto inputDto, CancellationToken token = default)
    {
        if (await dbContext.VideoTags.AnyAsync(t => t.Code == inputDto.Code, token))
        {
            return ReturnDataModel<VTagModel>.FailResult("标签编码已存在", ReturnDataCode.BadRequest);
        }

        var tag = new VideoTag
        {
            Name = inputDto.Name,
            Code = inputDto.Code,
            Description = inputDto.Description,
            SortOrder = inputDto.SortOrder,
            CreatedAt = DateTime.Now
        };

        dbContext.VideoTags.Add(tag);
        await dbContext.SaveChangesAsync(token);

        logger.LogInformation("创建标签成功: {Code}", inputDto.Code);

        var result = new VTagModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Code = tag.Code,
            Description = tag.Description,
            SortOrder = tag.SortOrder,
            CreatedAt = tag.CreatedAt,
            VideoCount = 0
        };

        return ReturnDataModel<VTagModel>.SuccessResult(result);
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    public async Task<ReturnDataModel<VTagModel>> UpdateTagAsync(UpdateTagInputDto inputDto, CancellationToken token = default)
    {
        var tag = await dbContext.VideoTags.FindAsync([inputDto.TagId], token);
        if (tag == null)
        {
            return ReturnDataModel<VTagModel>.FailResult("标签不存在", ReturnDataCode.NotFound);
        }

        tag.Name = inputDto.Name;
        tag.Description = inputDto.Description;
        tag.SortOrder = inputDto.SortOrder;
        tag.UpdatedAt = DateTime.Now;

        await dbContext.SaveChangesAsync(token);

        var videoCount = await dbContext.VideoTagMappings.CountAsync(m => m.TagId == inputDto.TagId, token);

        var result = new VTagModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Code = tag.Code,
            Description = tag.Description,
            SortOrder = tag.SortOrder,
            CreatedAt = tag.CreatedAt,
            VideoCount = videoCount
        };

        logger.LogInformation("更新标签成功: {TagId}", inputDto.TagId);
        return ReturnDataModel<VTagModel>.SuccessResult(result);
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    public async Task<ReturnDataModel<string>> DeleteTagAsync(DeleteTagInputDto inputDto, CancellationToken token = default)
    {
        var tag = await dbContext.VideoTags.FindAsync([inputDto.TagId], token);
        if (tag == null)
        {
            return ReturnDataModel<string>.FailResult("标签不存在", ReturnDataCode.NotFound);
        }

        tag.IsDeleted = true;
        tag.UpdatedAt = DateTime.Now;
        await dbContext.SaveChangesAsync(token);

        logger.LogInformation("删除标签成功: {TagId}", inputDto.TagId);
        return ReturnDataModel<string>.SuccessResult(tag.Id.ToString(), "删除成功");
    }
}
