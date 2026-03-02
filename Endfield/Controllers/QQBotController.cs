using System.Text.Json;
using Endfield.Api.Models.InputDto.QQBot;
using Endfield.Api.Models.QQBot;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Endfield.Api.Controllers;

/// <summary>
/// QQ机器人Webhook回调控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QQBotController : ControllerBase
{
    private readonly IQQBotService _qqBotService;
    private readonly ILogger<QQBotController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public QQBotController(
        IQQBotService qqBotService,
        ILogger<QQBotController> logger)
    {
        _qqBotService = qqBotService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Webhook回调端点
    /// 接收QQ开放平台推送的事件
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] QQBotCallbackInputDto inputDto)
    {
        try
        {
            Log.Information("收到Webhook回调 - OpCode: {OpCode}, EventType: {EventType}, Sequence: {Sequence}",
                inputDto.OpCode, inputDto.EventType, inputDto.Sequence);

            // 处理回调地址验证
            if (inputDto.OpCode == QQWebhookOpCodes.Validation)
            {
                return HandleValidation(inputDto);
            }

            // 处理事件分发
            if (inputDto.OpCode == QQWebhookOpCodes.Dispatch)
            {
                return await HandleDispatchAsync(inputDto);
            }

            Log.Warning("未处理的操作码: {OpCode}", inputDto.OpCode);
            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "处理Webhook回调失败");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 处理回调地址验证
    /// </summary>
    private IActionResult HandleValidation(QQBotCallbackInputDto inputDto)
    {
        try
        {
            // 从 Data 中解析验证请求
            var dataJson = inputDto.Data?.ToString();
            if (string.IsNullOrEmpty(dataJson))
            {
                Log.Warning("验证请求数据为空");
                return BadRequest();
            }

            var validationInput = JsonSerializer.Deserialize<QQBotValidationInputDto>(dataJson, _jsonOptions);
            if (validationInput == null)
            {
                Log.Warning("无法解析验证请求");
                return BadRequest();
            }

            Log.Information("收到回调地址验证请求 - PlainToken: {PlainToken}, EventTs: {EventTs}",
                validationInput.PlainToken, validationInput.EventTs);

            // 计算签名
            var signature = _qqBotService.CalculateSignature(validationInput.EventTs, validationInput.PlainToken);

            var response = new
            {
                plain_token = validationInput.PlainToken,
                signature = signature
            };

            Log.Information("回调地址验证响应 - PlainToken: {PlainToken}, Signature: {Signature}",
                response.plain_token, response.signature);

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "处理回调地址验证失败");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 处理事件分发
    /// </summary>
    private async Task<IActionResult> HandleDispatchAsync(QQBotCallbackInputDto inputDto)
    {
        try
        {
            var eventType = inputDto.EventType;
            Log.Information("收到事件 - 类型: {EventType}, 序号: {Sequence}", eventType, inputDto.Sequence);

            if (inputDto.Data == null)
            {
                Log.Warning("事件数据为空");
                return Ok(new { code = 0 });
            }

            var dataJson = inputDto.Data.ToString();

            switch (eventType)
            {
                case QQEventTypes.AtMessageCreate:
                case QQEventTypes.GroupAtMessageCreate:
                case QQEventTypes.C2CMessageCreate:
                    var messageInput = JsonSerializer.Deserialize<QQBotMessageInputDto>(dataJson!, _jsonOptions);
                    if (messageInput != null)
                    {
                        await HandleMessageEventAsync(messageInput, eventType!);
                    }
                    break;

                default:
                    Log.Debug("未处理的事件类型: {EventType}", eventType);
                    break;
            }

            // 返回HTTP回调ACK
            return Ok(new { code = 0 });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "处理事件分发失败");
            return Ok(new { code = 0 }); // 仍然返回成功，避免平台重试
        }
    }

    /// <summary>
    /// 处理消息事件
    /// </summary>
    private async Task HandleMessageEventAsync(QQBotMessageInputDto messageInput, string eventType)
    {
        try
        {
            Log.Information("处理消息 - ID: {MessageId}, 类型: {EventType}, 内容: {Content}",
                messageInput.Id, eventType, messageInput.Content);

            // 原样返回消息（移除@部分）
            var content = CleanMessageContent(messageInput);

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "你好！";
            }

            // 根据消息类型发送回复
            if (eventType == QQEventTypes.AtMessageCreate && !string.IsNullOrEmpty(messageInput.ChannelId))
            {
                // 频道消息
                await _qqBotService.SendChannelMessageAsync(messageInput.ChannelId, content, messageInput.Id);
            }
            else if (eventType == QQEventTypes.GroupAtMessageCreate && !string.IsNullOrEmpty(messageInput.GroupId))
            {
                // 群消息
                await _qqBotService.SendGroupMessageAsync(messageInput.GroupId, content, messageInput.Id);
            }
            else if (eventType == QQEventTypes.C2CMessageCreate)
            {
                // 私信消息
                var openid = messageInput.Author?.UserOpenId ?? messageInput.Author?.Id ?? "";
                if (!string.IsNullOrEmpty(openid))
                {
                    await _qqBotService.SendC2CMessageAsync(openid, content, messageInput.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "处理消息事件失败");
        }
    }

    /// <summary>
    /// 清理消息内容（移除@部分）
    /// </summary>
    private static string CleanMessageContent(QQBotMessageInputDto message)
    {
        var content = message.Content;
        if (message.Mentions != null)
        {
            foreach (var mention in message.Mentions)
            {
                if (!string.IsNullOrEmpty(mention.Id))
                {
                    content = content.Replace($"<@{mention.Id}>", "").Trim();
                }
            }
        }
        return content.Trim();
    }

    /// <summary>
    /// 健康检查端点
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
