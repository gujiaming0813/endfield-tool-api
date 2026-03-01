using System.Text.Json;
using System.Text.RegularExpressions;
using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Services;

/// <summary>
/// B站视频服务接口
/// </summary>
public interface IBilibiliService
{
    /// <summary>
    /// 根据BV号或链接获取视频信息
    /// </summary>
    /// <param name="input">BV号或视频链接</param>
    /// <param name="forceRefresh">是否强制刷新（跳过缓存）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>视频信息</returns>
    Task<BilibiliVideoInfo?> GetVideoInfoAsync(string input, bool forceRefresh = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// B站视频服务实现
/// </summary>
public partial class BilibiliService : IBilibiliService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BilibiliService> _logger;
    private readonly AppDbContext _dbContext;

    public BilibiliService(
        IHttpClientFactory httpClientFactory,
        ILogger<BilibiliService> logger,
        AppDbContext dbContext)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<BilibiliVideoInfo?> GetVideoInfoAsync(string input, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        // 从输入中提取BV号
        var bvid = ExtractBvid(input);
        if (string.IsNullOrEmpty(bvid))
        {
            _logger.LogWarning("无法从输入中提取有效的BV号: {Input}", input);
            return null;
        }

        // 1. 先从数据库查找缓存（非强制刷新时）
        if (!forceRefresh)
        {
            var cachedVideo = await _dbContext.BilibiliVideos
                .AsNoTracking()
                .Include(v => v.VideoTagMappings)
                .ThenInclude(m => m.Tag)
                .FirstOrDefaultAsync(v => v.Bvid == bvid, cancellationToken);

            if (cachedVideo != null)
            {
                _logger.LogInformation("从数据库缓存获取视频信息: {Bvid}", bvid);
                return MapToVideoInfo(cachedVideo);
            }
        }

        // 2. 调用B站API获取视频信息
        var videoInfo = await FetchFromApiAsync(bvid, cancellationToken);
        if (videoInfo == null)
        {
            return null;
        }

        // 3. 保存或更新到数据库
        await SaveToDatabaseAsync(videoInfo, cancellationToken);

        return videoInfo;
    }

    /// <summary>
    /// 从B站API获取视频信息
    /// </summary>
    private async Task<BilibiliVideoInfo?> FetchFromApiAsync(string bvid, CancellationToken cancellationToken)
    {
        try
        {
            var apiUrl = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            var response = await _httpClient.GetStringAsync(apiUrl, cancellationToken);
            var jsonDoc = JsonDocument.Parse(response);

            var root = jsonDoc.RootElement;
            var code = root.GetProperty("code").GetInt32();

            if (code != 0)
            {
                var message = root.TryGetProperty("message", out var msgElem) ? msgElem.GetString() : "未知错误";
                _logger.LogWarning("B站API返回错误: Code={Code}, Message={Message}", code, message);
                return null;
            }

            var data = root.GetProperty("data");
            return ParseVideoInfo(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取B站视频信息失败，BV号: {Bvid}", bvid);
            return null;
        }
    }

    /// <summary>
    /// 保存视频信息到数据库
    /// </summary>
    private async Task SaveToDatabaseAsync(BilibiliVideoInfo videoInfo, CancellationToken cancellationToken)
    {
        try
        {
            // 检查是否已存在
            var existingVideo = await _dbContext.BilibiliVideos
                .FirstOrDefaultAsync(v => v.Bvid == videoInfo.Bvid, cancellationToken);

            if (existingVideo != null)
            {
                // 更新现有记录
                existingVideo.Title = videoInfo.Title;
                existingVideo.Cover = videoInfo.Cover;
                existingVideo.Description = videoInfo.Description;
                existingVideo.Duration = videoInfo.Duration;
                existingVideo.OwnerName = videoInfo.OwnerName;
                existingVideo.ViewCount = videoInfo.ViewCount;
                existingVideo.LikeCount = videoInfo.LikeCount;
                existingVideo.PublishTime = videoInfo.PublishTime;
                existingVideo.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("更新数据库中的视频信息: {Bvid}", videoInfo.Bvid);
            }
            else
            {
                // 新增记录
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.BilibiliVideos.Add(entity);
                _logger.LogInformation("保存视频信息到数据库: {Bvid}", videoInfo.Bvid);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存视频信息到数据库失败: {Bvid}", videoInfo.Bvid);
        }
    }

    /// <summary>
    /// 将实体映射为响应模型
    /// </summary>
    private static BilibiliVideoInfo MapToVideoInfo(BilibiliVideo entity)
    {
        return new BilibiliVideoInfo
        {
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
            Tags = entity.VideoTagMappings.Select(m => new TagInfo
            {
                Id = m.Tag.Id,
                Name = m.Tag.Name,
                Code = m.Tag.Code
            }).ToList()
        };
    }

    /// <summary>
    /// 从输入字符串中提取BV号
    /// 支持格式：
    /// - 纯BV号：BV1xx411c7mD
    /// - 短链接：https://b23.tv/BV1xx411c7mD
    /// - 标准链接：https://www.bilibili.com/video/BV1xx411c7mD
    /// </summary>
    private static string? ExtractBvid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        // BV号正则：以BV开头，后跟10-12个字符（字母和数字）
        var bvidPattern = BvidRegex();
        var match = bvidPattern.Match(input);

        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// 解析B站API返回的视频数据
    /// </summary>
    private static BilibiliVideoInfo ParseVideoInfo(JsonElement data)
    {
        var bvid = data.GetProperty("bvid").GetString()!;
        var title = data.GetProperty("title").GetString()!;
        var cover = data.GetProperty("pic").GetString()!;
        var description = data.TryGetProperty("desc", out var descElem) ? descElem.GetString() : null;
        var duration = data.GetProperty("duration").GetInt32();
        var owner = data.GetProperty("owner");
        var ownerName = owner.GetProperty("name").GetString()!;

        // 获取统计数据
        long viewCount = 0, likeCount = 0;
        if (data.TryGetProperty("stat", out var stat))
        {
            viewCount = stat.TryGetProperty("view", out var viewElem) ? viewElem.GetInt64() : 0;
            likeCount = stat.TryGetProperty("like", out var likeElem) ? likeElem.GetInt64() : 0;
        }

        // 发布时间
        var publishTime = data.TryGetProperty("pubdate", out var pubDateElem)
            ? DateTimeOffset.FromUnixTimeSeconds(pubDateElem.GetInt64()).LocalDateTime
            : DateTime.MinValue;

        return new BilibiliVideoInfo
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
}
