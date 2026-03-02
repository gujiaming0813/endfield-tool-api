using System.Text.Json;
using Endfield.Api.Models.QQBot;
using Endfield.Api.Services;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Callback()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogInformation("收到Webhook回调: {Body}", body);

            var payload = JsonSerializer.Deserialize<QQWebhookPayload>(body, _jsonOptions);
            if (payload == null)
            {
                _logger.LogWarning("无法解析Webhook Payload");
                return BadRequest();
            }

            // 处理回调地址验证
            if (payload.OpCode == QQWebhookOpCodes.Validation)
            {
                return HandleValidation(payload);
            }

            // 处理事件分发
            if (payload.OpCode == QQWebhookOpCodes.Dispatch)
            {
                return await HandleDispatchAsync(payload);
            }

            _logger.LogWarning("未处理的操作码: {OpCode}", payload.OpCode);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理Webhook回调失败");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 处理回调地址验证
    /// </summary>
    private IActionResult HandleValidation(QQWebhookPayload payload)
    {
        try
        {
            var dataJson = payload.Data?.ToString();
            if (string.IsNullOrEmpty(dataJson))
            {
                _logger.LogWarning("验证请求数据为空");
                return BadRequest();
            }

            var validationRequest = JsonSerializer.Deserialize<QQValidationRequest>(dataJson, _jsonOptions);
            if (validationRequest == null)
            {
                _logger.LogWarning("无法解析验证请求");
                return BadRequest();
            }

            _logger.LogInformation("收到回调地址验证请求 - PlainToken: {PlainToken}, EventTs: {EventTs}",
                validationRequest.PlainToken, validationRequest.EventTs);

            // 计算签名
            var signature = _qqBotService.CalculateSignature(validationRequest.EventTs, validationRequest.PlainToken);

            var response = new QQValidationResponse
            {
                PlainToken = validationRequest.PlainToken,
                Signature = signature
            };

            _logger.LogInformation("回调地址验证响应 - PlainToken: {PlainToken}, Signature: {Signature}",
                response.PlainToken, response.Signature);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理回调地址验证失败");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 处理事件分发
    /// </summary>
    private async Task<IActionResult> HandleDispatchAsync(QQWebhookPayload payload)
    {
        try
        {
            var eventType = payload.EventType;
            _logger.LogInformation("收到事件 - 类型: {EventType}, 序号: {Sequence}", eventType, payload.Sequence);

            if (payload.Data == null)
            {
                _logger.LogWarning("事件数据为空");
                return Ok();
            }

            var dataJson = payload.Data.ToString();

            switch (eventType)
            {
                case QQEventTypes.AtMessageCreate:
                case QQEventTypes.GroupAtMessageCreate:
                    var message = JsonSerializer.Deserialize<QQMessageEvent>(dataJson!, _jsonOptions);
                    if (message != null)
                    {
                        await _qqBotService.HandleMessageEventAsync(message, eventType!);
                    }
                    break;

                case QQEventTypes.C2CMessageCreate:
                    var c2cMessage = JsonSerializer.Deserialize<QQMessageEvent>(dataJson!, _jsonOptions);
                    if (c2cMessage != null)
                    {
                        await _qqBotService.HandleMessageEventAsync(c2cMessage, eventType!);
                    }
                    break;

                default:
                    _logger.LogDebug("未处理的事件类型: {EventType}", eventType);
                    break;
            }

            // 返回HTTP回调ACK
            return Ok(new { code = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理事件分发失败");
            return Ok(new { code = 0 }); // 仍然返回成功，避免平台重试
        }
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
