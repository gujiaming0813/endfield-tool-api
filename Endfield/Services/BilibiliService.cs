using System.Text.Json;
using System.Text.RegularExpressions;
using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Models.InputDto.Video;
using Endfield.Api.Models.ViewModel.Common;
using Endfield.Api.Models.ViewModel.Tag;
using Endfield.Api.Models.ViewModel.Video;
using Endfield.Api.Share.Enums;
using Endfield.Api.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Services;

/// <summary>
/// B站视频服务实现
/// </summary>
public partial class BilibiliService(
    IHttpClientFactory httpClientFactory,
    ILogger<BilibiliService> logger,
    AppDbContext dbContext) : IBilibiliService
{
    private readonly HttpClient _httpClient = InitializeHttpClient(httpClientFactory);

    private static HttpClient InitializeHttpClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
        return client;
    }

    /// <summary>
    /// 导入视频
    /// </summary>
    public async Task<ReturnDataModel<VVideoInfoModel>> ImportVideoAsync(ImportVideoInputDto inputDto, CancellationToken token = default)
    {
        var bvid = ExtractBvid(inputDto.Input);
        if (string.IsNullOrEmpty(bvid))
        {
            return ReturnDataModel<VVideoInfoModel>.FailResult("无法从输入中提取有效的BV号", ReturnDataCode.BadRequest);
        }

        // 检查视频是否已存在
        var existingVideo = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .FirstOrDefaultAsync(v => v.Bvid == bvid, token);

        if (existingVideo != null)
        {
            // 如果传入了新标签，更新标签
            if (inputDto.TagIds != null && inputDto.TagIds.Count != 0)
            {
                await UpdateVideoTagsAsync(existingVideo, inputDto.TagIds, token);
            }

            return ReturnDataModel<VVideoInfoModel>.SuccessResult(MapToViewModel(existingVideo));
        }

        // 从B站API获取视频信息
        var videoInfo = await FetchFromApiAsync(bvid, token);
        if (videoInfo == null)
        {
            return ReturnDataModel<VVideoInfoModel>.FailResult("未找到视频信息，请检查BV号或链接是否正确", ReturnDataCode.NotFound);
        }

        // 保存到数据库
        var entity = new BilibiliVideo
        {
            Bvid = videoInfo.Bvid,
            Title = videoInfo.Title,
            Cover = videoInfo.Cover,
            Description = videoInfo.Description,
            Duration = videoInfo.Duration,
            OwnerName = videoInfo.OwnerName,
            Url = videoInfo.Url,
            ViewCount = videoInfo.ViewCount,
            LikeCount = videoInfo.LikeCount,
            PublishTime = videoInfo.PublishTime,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.BilibiliVideos.Add(entity);
        await dbContext.SaveChangesAsync(token);

        // 设置标签
        if (inputDto.TagIds != null && inputDto.TagIds.Count != 0)
        {
            await UpdateVideoTagsAsync(entity, inputDto.TagIds, token);
        }

        // 重新查询以获取完整的标签信息
        var savedVideo = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .FirstAsync(v => v.Id == entity.Id, token);

        logger.LogInformation("导入视频成功: {Bvid}", bvid);
        return ReturnDataModel<VVideoInfoModel>.SuccessResult(MapToViewModel(savedVideo));
    }

    /// <summary>
    /// 更新视频
    /// </summary>
    public async Task<ReturnDataModel<VVideoInfoModel>> UpdateVideoAsync(UpdateVideoInputDto inputDto, CancellationToken token = default)
    {
        var video = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .FirstOrDefaultAsync(v => v.Id == inputDto.VideoId, token);

        if (video == null)
        {
            return ReturnDataModel<VVideoInfoModel>.FailResult("视频不存在", ReturnDataCode.NotFound);
        }

        // 如果需要刷新视频信息
        if (inputDto.RefreshInfo)
        {
            var videoInfo = await FetchFromApiAsync(video.Bvid, token);
            if (videoInfo != null)
            {
                video.Title = videoInfo.Title;
                video.Cover = videoInfo.Cover;
                video.Description = videoInfo.Description;
                video.Duration = videoInfo.Duration;
                video.OwnerName = videoInfo.OwnerName;
                video.ViewCount = videoInfo.ViewCount;
                video.LikeCount = videoInfo.LikeCount;
                video.PublishTime = videoInfo.PublishTime;
            }
        }

        // 更新标签
        if (inputDto.TagIds != null)
        {
            await UpdateVideoTagsAsync(video, inputDto.TagIds, token);
        }

        video.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(token);

        // 重新查询以获取完整的标签信息（包括新添加的标签导航属性）
        var updatedVideo = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .FirstAsync(v => v.Id == inputDto.VideoId, token);

        logger.LogInformation("更新视频成功: {VideoId}", inputDto.VideoId);
        return ReturnDataModel<VVideoInfoModel>.SuccessResult(MapToViewModel(updatedVideo));
    }

    /// <summary>
    /// 分页查询视频列表
    /// </summary>
    public async Task<ReturnDataModel<VBasePagingViewModel<VVideoInfoModel>>> QueryVideoListAsync(
        QueryVideoListInputDto inputDto, CancellationToken token = default)
    {
        var query = dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .AsNoTracking();

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(inputDto.Keyword))
        {
            var keyword = inputDto.Keyword.Trim();
            query = query.Where(v => v.Title.Contains(keyword) || (v.Description != null && v.Description.Contains(keyword)));
        }

        // 标签筛选（AND关系）
        if (inputDto.TagIds != null && inputDto.TagIds.Count != 0)
        {
            var tagIds = inputDto.TagIds.Distinct().ToList();
            var requiredCount = tagIds.Count;

            // 使用子查询：查找拥有所有指定标签的视频
            query = query.Where(v =>
                dbContext.VideoTagMappings
                    .Where(m => m.VideoId == v.Id && tagIds.Contains(m.TagId))
                    .Count() == requiredCount);
        }

        var total = await query.CountAsync(token);

        var videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((inputDto.Page - 1) * inputDto.PageSize)
            .Take(inputDto.PageSize)
            .ToListAsync(token);

        var result = new VBasePagingViewModel<VVideoInfoModel>
        {
            Total = total,
            Page = inputDto.Page,
            PageSize = inputDto.PageSize,
            Rows = videos.Select(MapToViewModel).ToList()
        };

        return ReturnDataModel<VBasePagingViewModel<VVideoInfoModel>>.SuccessResult(result);
    }

    /// <summary>
    /// 获取视频详情
    /// </summary>
    public async Task<ReturnDataModel<VVideoInfoModel>> GetVideoByIdAsync(int videoId, CancellationToken token = default)
    {
        var video = await dbContext.BilibiliVideos
            .Include(v => v.VideoTagMappings)
            .ThenInclude(m => m.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == videoId, token);

        if (video == null)
        {
            return ReturnDataModel<VVideoInfoModel>.FailResult("视频不存在", ReturnDataCode.NotFound);
        }

        return ReturnDataModel<VVideoInfoModel>.SuccessResult(MapToViewModel(video));
    }

    /// <summary>
    /// 删除视频
    /// </summary>
    public async Task<ReturnDataModel<string>> DeleteVideoAsync(DeleteVideoInputDto inputDto, CancellationToken token = default)
    {
        var video = await dbContext.BilibiliVideos.FirstOrDefaultAsync(v => v.Id == inputDto.VideoId, token);
        if (video == null)
        {
            return ReturnDataModel<string>.FailResult("视频不存在", ReturnDataCode.NotFound);
        }

        video.IsDeleted = true;
        video.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(token);

        logger.LogInformation("删除视频成功: {VideoId}", inputDto.VideoId);
        return ReturnDataModel<string>.SuccessResult(video.Id.ToString(), "删除成功");
    }

    #region 私有方法

    /// <summary>
    /// 更新视频标签
    /// </summary>
    private async Task UpdateVideoTagsAsync(BilibiliVideo video, List<int> tagIds, CancellationToken token)
    {
        // 验证标签是否存在
        var existingTagIds = await dbContext.VideoTags
            .Where(t => tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(token);

        // 移除所有旧标签
        dbContext.VideoTagMappings.RemoveRange(video.VideoTagMappings);

        // 添加新标签
        foreach (var tagId in tagIds.Distinct().Where(id => existingTagIds.Contains(id)))
        {
            dbContext.VideoTagMappings.Add(new VideoTagMapping
            {
                VideoId = video.Id,
                TagId = tagId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(token);
    }

    /// <summary>
    /// 从B站API获取视频信息
    /// </summary>
    private async Task<VVideoInfoModel?> FetchFromApiAsync(string bvid, CancellationToken token)
    {
        try
        {
            var apiUrl = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            var response = await _httpClient.GetStringAsync(apiUrl, token);
            var jsonDoc = JsonDocument.Parse(response);

            var root = jsonDoc.RootElement;
            var code = root.GetProperty("code").GetInt32();

            if (code != 0)
            {
                var message = root.TryGetProperty("message", out var msgElem) ? msgElem.GetString() : "未知错误";
                logger.LogWarning("B站API返回错误: Code={Code}, Message={Message}", code, message);
                return null;
            }

            var data = root.GetProperty("data");
            return ParseVideoInfo(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取B站视频信息失败，BV号: {Bvid}", bvid);
            return null;
        }
    }

    /// <summary>
    /// 将实体映射为视图模型
    /// </summary>
    private static VVideoInfoModel MapToViewModel(BilibiliVideo entity)
    {
        return new VVideoInfoModel
        {
            Id = entity.Id,
            Bvid = entity.Bvid,
            Title = entity.Title,
            Cover = entity.Cover,
            Description = entity.Description,
            Duration = entity.Duration,
            OwnerName = entity.OwnerName,
            Url = entity.Url,
            ViewCount = entity.ViewCount,
            LikeCount = entity.LikeCount,
            PublishTime = entity.PublishTime,
            Tags = entity.VideoTagMappings.Select(m => new VTagInfoModel
            {
                Id = m.Tag.Id,
                Name = m.Tag.Name,
                Code = m.Tag.Code
            }).ToList()
        };
    }

    /// <summary>
    /// 从输入字符串中提取BV号
    /// </summary>
    private static string? ExtractBvid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();
        var match = BvidRegex().Match(input);
        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// 解析B站API返回的视频数据
    /// </summary>
    private static VVideoInfoModel ParseVideoInfo(JsonElement data)
    {
        var bvid = data.GetProperty("bvid").GetString()!;
        var title = data.GetProperty("title").GetString()!;
        var cover = data.GetProperty("pic").GetString()!;
        var description = data.TryGetProperty("desc", out var descElem) ? descElem.GetString() : null;
        var duration = data.GetProperty("duration").GetInt32();
        var owner = data.GetProperty("owner");
        var ownerName = owner.GetProperty("name").GetString()!;

        long viewCount = 0, likeCount = 0;
        if (data.TryGetProperty("stat", out var stat))
        {
            viewCount = stat.TryGetProperty("view", out var viewElem) ? viewElem.GetInt64() : 0;
            likeCount = stat.TryGetProperty("like", out var likeElem) ? likeElem.GetInt64() : 0;
        }

        var publishTime = data.TryGetProperty("pubdate", out var pubDateElem)
            ? DateTimeOffset.FromUnixTimeSeconds(pubDateElem.GetInt64()).LocalDateTime
            : DateTime.MinValue;

        return new VVideoInfoModel
        {
            Bvid = bvid,
            Title = title,
            Cover = cover,
            Description = description,
            Duration = duration,
            OwnerName = ownerName,
            Url = $"https://www.bilibili.com/video/{bvid}",
            ViewCount = viewCount,
            LikeCount = likeCount,
            PublishTime = publishTime
        };
    }

    [GeneratedRegex(@"BV[a-zA-Z0-9]{10,12}", RegexOptions.IgnoreCase)]
    private static partial Regex BvidRegex();

    #endregion
}
