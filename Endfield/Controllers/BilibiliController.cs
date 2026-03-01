using Endfield.Api.Data;
using Endfield.Api.Models;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Controllers;

/// <summary>
/// B站视频信息接口
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BilibiliController(IBilibiliService bilibiliService, AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// 根据BV号或链接获取视频信息
    /// </summary>
    /// <param name="input">BV号或视频链接</param>
    /// <param name="forceRefresh">是否强制刷新（跳过缓存）</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpGet("video")]
    [ProducesResponseType<BilibiliVideoInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideoInfo(
        [FromQuery] string input,
        [FromQuery] bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return BadRequest(new { error = "请提供BV号或视频链接" });
        }

        var videoInfo = await bilibiliService.GetVideoInfoAsync(input, forceRefresh, cancellationToken);

        if (videoInfo is null)
        {
            return NotFound(new { error = "未找到视频信息，请检查BV号或链接是否正确" });
        }

        return Ok(videoInfo);
    }

    /// <summary>
    /// 设置视频标签（替换原有标签）
    /// </summary>
    /// <param name="bvid">视频BV号</param>
    /// <param name="tagIds">标签ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpPut("video/{bvid}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVideoTags(
        string bvid,
        [FromBody] List<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        var video = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .FirstOrDefaultAsync(v => v.Bvid == bvid, cancellationToken);

        if (video == null)
        {
            return NotFound(new { error = "视频不存在" });
        }

        // 验证所有标签是否存在
        var existingTagIds = await dbContext.VideoTags
            .Where(t => tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var invalidTagIds = tagIds.Except(existingTagIds).ToList();
        if (invalidTagIds.Count != 0)
        {
            return BadRequest(new { error = $"以下标签不存在: {string.Join(", ", invalidTagIds)}" });
        }

        // 删除原有标签关联
        dbContext.VideoTagMappings.RemoveRange(video.VideoTagMappings);

        // 添加新的标签关联
        foreach (var tagId in tagIds.Distinct())
        {
            dbContext.VideoTagMappings.Add(new Entities.VideoTagMapping
            {
                VideoId = video.Id,
                TagId = tagId,
                CreatedAt = DateTime.UtcNow
            });
        }

        video.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "标签设置成功", bvid, tagIds });
    }

    /// <summary>
    /// 为视频添加标签（追加）
    /// </summary>
    /// <param name="bvid">视频BV号</param>
    /// <param name="tagIds">标签ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpPost("video/{bvid}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddVideoTags(
        string bvid,
        [FromBody] List<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        var video = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .FirstOrDefaultAsync(v => v.Bvid == bvid, cancellationToken);

        if (video == null)
        {
            return NotFound(new { error = "视频不存在" });
        }

        // 获取已存在的标签ID
        var existingTagIds = video.VideoTagMappings.Select(m => m.TagId).ToHashSet();

        // 验证新标签是否存在
        var validTagIds = await dbContext.VideoTags
            .Where(t => tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var invalidTagIds = tagIds.Except(validTagIds).ToList();
        if (invalidTagIds.Count != 0)
        {
            return BadRequest(new { error = $"以下标签不存在: {string.Join(", ", invalidTagIds)}" });
        }

        // 只添加不存在的标签
        var newTagIds = tagIds.Distinct().Where(id => !existingTagIds.Contains(id)).ToList();

        foreach (var tagId in newTagIds)
        {
            dbContext.VideoTagMappings.Add(new Entities.VideoTagMapping
            {
                VideoId = video.Id,
                TagId = tagId,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (newTagIds.Count != 0)
        {
            video.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { message = "标签添加成功", bvid, addedTagIds = newTagIds });
    }

    /// <summary>
    /// 移除视频的指定标签
    /// </summary>
    /// <param name="bvid">视频BV号</param>
    /// <param name="tagIds">要移除的标签ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpDelete("video/{bvid}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveVideoTags(
        string bvid,
        [FromBody] List<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        var video = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .FirstOrDefaultAsync(v => v.Bvid == bvid, cancellationToken);

        if (video == null)
        {
            return NotFound(new { error = "视频不存在" });
        }

        // 删除指定的标签关联
        var mappingsToRemove = video.VideoTagMappings
            .Where(m => tagIds.Contains(m.TagId))
            .ToList();

        if (mappingsToRemove.Count != 0)
        {
            dbContext.VideoTagMappings.RemoveRange(mappingsToRemove);
            video.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { message = "标签移除成功", bvid, removedTagIds = mappingsToRemove.Select(m => m.TagId).ToList() });
    }

    /// <summary>
    /// 删除视频（软删除）
    /// </summary>
    /// <param name="bvid">视频BV号</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpDelete("video/{bvid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVideo(string bvid, CancellationToken cancellationToken = default)
    {
        var video = await dbContext.BilibiliVideos.FirstOrDefaultAsync(v => v.Bvid == bvid, cancellationToken);
        if (video == null)
        {
            return NotFound(new { error = "视频不存在" });
        }

        video.IsDeleted = true;
        video.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// 获取视频列表（支持按标签筛选）
    /// </summary>
    /// <param name="tagId">标签ID（可选，多个标签用逗号分隔）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    [HttpGet("videos")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVideos(
        [FromQuery] string? tagId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .AsNoTracking();

        // 按标签筛选
        if (!string.IsNullOrWhiteSpace(tagId))
        {
            var tagIds = tagId.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (tagIds.Count != 0)
            {
                query = query.Where(v => v.VideoTagMappings.Any(m => tagIds.Contains(m.TagId)));
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new BilibiliVideoInfo
            {
                Bvid = v.Bvid,
                Title = v.Title,
                Cover = v.Cover,
                Description = v.Description,
                Duration = v.Duration,
                OwnerName = v.OwnerName,
                Url = v.Url,
                ViewCount = v.ViewCount,
                LikeCount = v.LikeCount,
                PublishTime = v.PublishTime,
                Tags = v.VideoTagMappings.Select(m => new TagInfo
                {
                    Id = m.Tag.Id,
                    Name = m.Tag.Name,
                    Code = m.Tag.Code
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            total,
            page,
            pageSize,
            data = videos
        });
    }
}
