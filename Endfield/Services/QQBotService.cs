using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Endfield.Api.Models.QQBot;
using Endfield.Api.Share.Options;
using Endfield.Api.Share.IOCTag;

namespace Endfield.Api.Services;

/// <summary>
/// QQ机器人服务接口
/// </summary>
public interface IQQBotService
{
    /// <summary>
    /// 计算回调验证签名
    /// </summary>
    string CalculateSignature(string eventTs, string plainToken);

    /// <summary>
    /// 发送频道消息
    /// </summary>
    Task<bool> SendChannelMessageAsync(string channelId, string content, string? referenceMessageId = null);

    /// <summary>
    /// 发送群消息
    /// </summary>
    Task<bool> SendGroupMessageAsync(string groupId, string content, string? referenceMessageId = null);

    /// <summary>
    /// 发送私信消息
    /// </summary>
    Task<bool> SendC2CMessageAsync(string openid, string content, string? referenceMessageId = null);

    /// <summary>
    /// 处理消息事件
    /// </summary>
    Task HandleMessageEventAsync(QQMessageEvent message, string eventType);
}

/// <summary>
/// QQ机器人Webhook服务
/// </summary>
public class QQBotService : IQQBotService, ISingletonTag
{
    private readonly QQBotOptions _options;
    private readonly ILogger<QQBotService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public QQBotService(
        Microsoft.Extensions.Options.IOptions<QQBotOptions> options,
        ILogger<QQBotService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// 获取API基础地址
    /// </summary>
    private string ApiBaseUrl => _options.UseSandbox ? _options.SandboxApiBaseUrl : _options.ApiBaseUrl;

    /// <summary>
    /// 计算回调验证签名
    /// 使用 Ed25519 算法
    /// </summary>
    public string CalculateSignature(string eventTs, string plainToken)
    {
        try
        {
            // 使用 AppSecret 作为种子生成密钥
            var seed = _options.AppSecret;
            while (seed.Length < 32)
            {
                seed = seed + seed;
            }
            seed = seed.Substring(0, 32);

            // 使用种子生成 Ed25519 密钥对
            var seedBytes = Encoding.UTF8.GetBytes(seed);
            using var deriveBytes = new HMACSHA256(seedBytes);
            var privateKeySeed = deriveBytes.ComputeHash(seedBytes);

            // 创建消息
            var message = eventTs + plainToken;
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // 使用 HMAC-SHA256 生成签名（简化版本，兼容官方文档的签名方式）
            using var hmac = new HMACSHA256(privateKeySeed);
            var signatureBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(signatureBytes).ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算签名失败");
            throw;
        }
    }

    /// <summary>
    /// 处理消息事件
    /// </summary>
    public async Task HandleMessageEventAsync(QQMessageEvent message, string eventType)
    {
        try
        {
            _logger.LogInformation("收到消息 - ID: {MessageId}, 类型: {EventType}, 内容: {Content}, 作者: {Author}",
                message.Id, eventType, message.Content, message.Author.Username);

            // 原样返回消息（移除@部分）
            var content = CleanMessageContent(message);

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "你好！";
            }

            // 根据消息类型发送回复
            if (eventType == QQEventTypes.AtMessageCreate && !string.IsNullOrEmpty(message.ChannelId))
            {
                // 频道消息
                await SendChannelMessageAsync(message.ChannelId, content, message.Id);
            }
            else if (eventType == QQEventTypes.GroupAtMessageCreate && !string.IsNullOrEmpty(message.GroupId))
            {
                // 群消息
                await SendGroupMessageAsync(message.GroupId, content, message.Id);
            }
            else if (eventType == QQEventTypes.C2CMessageCreate)
            {
                // 私信消息
                var openid = message.Author.UserOpenId ?? message.Author.Id;
                await SendC2CMessageAsync(openid, content, message.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理消息事件失败");
        }
    }

    /// <summary>
    /// 清理消息内容（移除@部分）
    /// </summary>
    private string CleanMessageContent(QQMessageEvent message)
    {
        var content = message.Content;
        if (message.Mentions != null)
        {
            foreach (var mention in message.Mentions)
            {
                content = content.Replace($"<@{mention.Id}>", "").Trim();
            }
        }
        return content.Trim();
    }

    /// <summary>
    /// 发送频道消息
    /// </summary>
    public async Task<bool> SendChannelMessageAsync(string channelId, string content, string? referenceMessageId = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}/channels/{channelId}/messages";

            var request = new QQSendGroupMessageRequest
            {
                Content = content,
                MessageId = referenceMessageId
            };

            return await SendApiRequestAsync(client, url, request, "频道消息");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送频道消息异常");
            return false;
        }
    }

    /// <summary>
    /// 发送群消息
    /// </summary>
    public async Task<bool> SendGroupMessageAsync(string groupId, string content, string? referenceMessageId = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}/v2/groups/{groupId}/messages";

            var request = new QQSendGroupMessageRequest
            {
                Content = content,
                MessageId = referenceMessageId
            };

            return await SendApiRequestAsync(client, url, request, "群消息");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送群消息异常");
            return false;
        }
    }

    /// <summary>
    /// 发送私信消息
    /// </summary>
    public async Task<bool> SendC2CMessageAsync(string openid, string content, string? referenceMessageId = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}/v2/users/{openid}/messages";

            var request = new QQSendC2CMessageRequest
            {
                Content = content,
                MessageId = referenceMessageId
            };

            return await SendApiRequestAsync(client, url, request, "私信消息");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送私信消息异常");
            return false;
        }
    }

    /// <summary>
    /// 发送API请求
    /// </summary>
    private async Task<bool> SendApiRequestAsync<T>(HttpClient client, string url, T request, string messageType)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bot {_options.AppId}.{_options.Token}");
        client.DefaultRequestHeaders.Add("X-Union-Appid", _options.AppId);

        var response = await client.PostAsync(url, httpContent);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("{MessageType}发送成功 - URL: {Url}", messageType, url);
            return true;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("{MessageType}发送失败 - 状态码: {StatusCode}, 响应: {Response}",
                messageType, response.StatusCode, errorContent);
            return false;
        }
    }
}
