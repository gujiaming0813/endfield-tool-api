using System.Text.Json;
using Endfield.Api.Data;
using Endfield.Api.Entities;
using Endfield.Api.Share.Enums;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Services;

/// <summary>
/// 视频刷新服务实现
/// </summary>
public class VideoRefreshService(
    IHttpClientFactory httpClientFactory,
    ILogger<VideoRefreshService> logger,
    AppDbContext dbContext) : IVideoRefreshService
{
    private readonly HttpClient _httpClient = InitializeHttpClient(httpClientFactory);

    /// <summary>
    /// 近一个月的天数
    /// </summary>
    private const int RecentDays = 30;

    /// <summary>
    /// 请求间隔（毫秒），避免触发B站限流
    /// </summary>
    private const int RequestDelayMs = 500;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    private const int MaxRetryCount = 3;

    /// <summary>
    /// 限流后等待时间（秒）
    /// </summary>
    private const int RateLimitWaitSeconds = 60;

    private static HttpClient InitializeHttpClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com");
        return client;
    }

    /// <summary>
    /// 刷新近一个月发布的视频信息
    /// </summary>
    public async Task RefreshRecentVideosAsync(CancellationToken token = default)
    {
        var cutoffDate = DateTime.Now.AddDays(-RecentDays);
        logger.LogInformation("开始刷新近 {Days} 天发布的视频，截止日期: {CutoffDate}", RecentDays, cutoffDate);

        // 查询需要刷新的视频：
        // 1. 发布时间在近一个月内
        // 2. 未被删除
        // 3. 刷新状态为待刷新，或者失败/限流但需要重试的
        var videosToRefresh = await dbContext.BilibiliVideos
            .Where(v => v.PublishTime >= cutoffDate
                && !v.IsDeleted
                && (v.RefreshStatus == VideoRefreshStatus.Pending
                    || (v.RefreshStatus == VideoRefreshStatus.Failed && v.RefreshRetryCount < MaxRetryCount)
                    || (v.RefreshStatus == VideoRefreshStatus.RateLimited && v.RefreshRetryCount < MaxRetryCount)))
            .OrderBy(v => v.LastRefreshTime ?? DateTime.MinValue)
            .ToListAsync(token);

        logger.LogInformation("找到 {Count} 个视频需要刷新", videosToRefresh.Count);

        var successCount = 0;
        var failedCount = 0;
        var rateLimitedCount = 0;

        foreach (var video in videosToRefresh)
        {
            if (token.IsCancellationRequested)
            {
                logger.LogWarning("刷新任务被取消");
                break;
            }

            try
            {
                var success = await RefreshSingleVideoAsync(video, token);

                if (success)
                {
                    successCount++;
                    video.RefreshStatus = VideoRefreshStatus.Success;
                    video.RefreshRetryCount = 0;
                }
                else
                {
                    // 如果状态不是 RateLimited（已在 RefreshSingleVideoAsync 中设置），则设置为 Failed
                    if (video.RefreshStatus != VideoRefreshStatus.RateLimited)
                    {
                        video.RefreshStatus = VideoRefreshStatus.Failed;
                    }
                    else
                    {
                        rateLimitedCount++;
                    }
                    video.RefreshRetryCount++;
                    failedCount++;
                }

                video.LastRefreshTime = DateTime.Now;
                await dbContext.SaveChangesAsync(token);

                // 请求间隔，避免限流
                await Task.Delay(RequestDelayMs, token);
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("刷新任务被取消");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "刷新视频 {VideoId} 时发生异常", video.Id);
                video.RefreshStatus = VideoRefreshStatus.Failed;
                video.RefreshRetryCount++;
                video.LastRefreshTime = DateTime.Now;
                failedCount++;
                await dbContext.SaveChangesAsync(token);
            }
        }

        logger.LogInformation("视频刷新完成：成功 {Success}，失败 {Failed}，限流 {RateLimited}",
            successCount, failedCount, rateLimitedCount);
    }

    /// <summary>
    /// 刷新单个视频信息
    /// </summary>
    private async Task<bool> RefreshSingleVideoAsync(BilibiliVideo video, CancellationToken token)
    {
        try
        {
            var apiUrl = $"https://api.bilibili.com/x/web-interface/view?bvid={video.Bvid}";
            var response = await _httpClient.GetStringAsync(apiUrl, token);
            var jsonDoc = JsonDocument.Parse(response);

            var root = jsonDoc.RootElement;
            var code = root.GetProperty("code").GetInt32();

            // 检查是否被限流
            if (code == -412)
            {
                logger.LogWarning("视频 {Bvid} 刷新时触发限流，等待 {WaitSeconds} 秒后重试",
                    video.Bvid, RateLimitWaitSeconds);
                video.RefreshStatus = VideoRefreshStatus.RateLimited;
                await Task.Delay(RateLimitWaitSeconds * 1000, token);
                return false;
            }

            if (code != 0)
            {
                var message = root.TryGetProperty("message", out var msgElem) ? msgElem.GetString() : "未知错误";
                logger.LogWarning("B站API返回错误: Code={Code}, Message={Message}, Bvid={Bvid}",
                    code, message, video.Bvid);
                return false;
            }

            var data = root.GetProperty("data");
            UpdateVideoInfo(video, data);

            logger.LogInformation("视频 {Bvid} 刷新成功", video.Bvid);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "请求B站API失败，Bvid: {Bvid}", video.Bvid);
            return false;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "解析B站API响应失败，Bvid: {Bvid}", video.Bvid);
            return false;
        }
    }

    /// <summary>
    /// 更新视频信息
    /// </summary>
    private static void UpdateVideoInfo(BilibiliVideo video, JsonElement data)
    {
        video.Title = data.GetProperty("title").GetString() ?? video.Title;
        video.Cover = data.GetProperty("pic").GetString() ?? video.Cover;
        video.Description = data.TryGetProperty("desc", out var descElem) ? descElem.GetString() : video.Description;
        video.Duration = data.GetProperty("duration").GetInt32();

        var owner = data.GetProperty("owner");
        video.OwnerName = owner.GetProperty("name").GetString() ?? video.OwnerName;

        if (data.TryGetProperty("stat", out var stat))
        {
            video.ViewCount = stat.TryGetProperty("view", out var viewElem) ? viewElem.GetInt64() : video.ViewCount;
            video.LikeCount = stat.TryGetProperty("like", out var likeElem) ? likeElem.GetInt64() : video.LikeCount;
        }

        if (data.TryGetProperty("pubdate", out var pubDateElem))
        {
            video.PublishTime = DateTimeOffset.FromUnixTimeSeconds(pubDateElem.GetInt64()).LocalDateTime;
        }
    }
}
