using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Controllers;

/// <summary>
/// 视频标签管理接口
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// 获取所有标签列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<VideoTagResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags()
    {
        var tags = await dbContext.VideoTags
            .OrderBy(t => t.SortOrder)
            .Select(t => new VideoTagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                VideoCount = t.VideoTagMappings.Count
            })
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// 根据ID获取标签详情
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VideoTagResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await dbContext.VideoTags
            .Where(t => t.Id == id)
            .Select(t => new VideoTagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                VideoCount = t.VideoTagMappings.Count
            })
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            return NotFound(new { error = "标签不存在" });
        }

        return Ok(tag);
    }

    /// <summary>
    /// 创建新标签
    /// </summary>
    [HttpPost]
    [ProducesResponseType<VideoTagResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        // 检查编码是否已存在
        if (await dbContext.VideoTags.AnyAsync(t => t.Code == request.Code))
        {
            return BadRequest(new { error = "标签编码已存在" });
        }

        var tag = new VideoTag
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.VideoTags.Add(tag);
        await dbContext.SaveChangesAsync();

        var response = new VideoTagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Code = tag.Code,
            Description = tag.Description,
            SortOrder = tag.SortOrder,
            CreatedAt = tag.CreatedAt,
            VideoCount = 0
        };

        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, response);
    }

    /// <summary>
    /// 更新标签信息
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<VideoTagResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        var tag = await dbContext.VideoTags.FindAsync(id);
        if (tag == null)
        {
            return NotFound(new { error = "标签不存在" });
        }

        tag.Name = request.Name;
        tag.Description = request.Description;
        tag.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync();

        var videoCount = await dbContext.VideoTagMappings.CountAsync(m => m.TagId == id);

        var response = new VideoTagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Code = tag.Code,
            Description = tag.Description,
            SortOrder = tag.SortOrder,
            CreatedAt = tag.CreatedAt,
            VideoCount = videoCount
        };

        return Ok(response);
    }

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var tag = await dbContext.VideoTags.FindAsync(id);
        if (tag == null)
        {
            return NotFound(new { error = "标签不存在" });
        }

        tag.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
