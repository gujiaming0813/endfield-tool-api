using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Endfield.Api.Models.QQBot;
using Endfield.Api.Share.Options;
using Endfield.Api.Share.IOCTag;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

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

    // Ed25519 种子大小
    private const int Ed25519SeedSize = 32;

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
    /// 使用 Ed25519 算法（按照官方Go示例实现）
    /// </summary>
    public string CalculateSignature(string eventTs, string plainToken)
    {
        try
        {
            // 1. 使用 AppSecret 作为种子
            var seed = _options.AppSecret;

            // 2. 扩展种子到至少 32 字节（模拟 Go 的 strings.Repeat）
            while (seed.Length < Ed25519SeedSize)
            {
                seed = seed + seed;
            }

            // 3. 截取前 32 字节作为种子
            seed = seed.Substring(0, Ed25519SeedSize);
            var seedBytes = Encoding.UTF8.GetBytes(seed);

            // 4. 从种子生成 Ed25519 私钥
            // Go 的 ed25519.GenerateKey 从 reader 读取 32 字节作为种子
            // 然后通过 SHA512 派生出私钥
            var privateKey = GenerateEd25519PrivateKeyFromSeed(seedBytes);

            // 5. 构建消息：eventTs + plainToken
            var message = eventTs + plainToken;
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // 6. 使用 Ed25519 签名
            var signature = SignEd25519(privateKey, messageBytes);

            // 7. 转换为十六进制字符串
            return Hex.ToHexString(signature).ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算签名失败");
            throw;
        }
    }

    /// <summary>
    /// 从种子生成 Ed25519 私钥
    /// 模拟 Go 的 ed25519.GenerateKey 行为
    /// </summary>
    private static Ed25519PrivateKeyParameters GenerateEd25519PrivateKeyFromSeed(byte[] seed)
    {
        // Ed25519 使用 SHA512 对种子进行哈希，前 32 字节作为私钥的 "a" 值
        // 后 32 字节用于生成公钥
        using var sha512 = SHA512.Create();
        var digest = sha512.ComputeHash(seed);

        // 按照 Ed25519 规范，对前 32 字节进行处理
        // 清除和设置特定位（clamp）
        digest[0] &= 0xF8;
        digest[31] &= 0x7F;
        digest[31] |= 0x40;

        // 取前 32 字节作为私钥种子
        var privateKeySeed = new byte[32];
        Array.Copy(digest, 0, privateKeySeed, 0, 32);

        return new Ed25519PrivateKeyParameters(privateKeySeed, 0);
    }

    /// <summary>
    /// 使用 Ed25519 私钥签名
    /// </summary>
    private static byte[] SignEd25519(Ed25519PrivateKeyParameters privateKey, byte[] message)
    {
        var signer = new Ed25519Signer();
        signer.Init(true, privateKey);
        signer.BlockUpdate(message, 0, message.Length);
        return signer.GenerateSignature();
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
