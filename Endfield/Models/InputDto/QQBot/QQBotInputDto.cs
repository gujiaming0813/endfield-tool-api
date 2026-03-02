using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Endfield.Api.Models.InputDto.QQBot;

/// <summary>
/// QQ机器人Webhook回调输入参数
/// </summary>
public record QQBotCallbackInputDto
{
    /// <summary>
    /// 事件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// 操作码
    /// </summary>
    [JsonPropertyName("op")]
    [Required]
    public required int OpCode { get; init; }

    /// <summary>
    /// 事件数据（原始JSON）
    /// </summary>
    [JsonPropertyName("d")]
    public object? Data { get; init; }

    /// <summary>
    /// 序列号
    /// </summary>
    [JsonPropertyName("s")]
    public long Sequence { get; init; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [JsonPropertyName("t")]
    public string? EventType { get; init; }
}

/// <summary>
/// 回调地址验证数据
/// </summary>
public record QQBotValidationInputDto
{
    /// <summary>
    /// 需要计算签名的字符串
    /// </summary>
    [JsonPropertyName("plain_token")]
    [Required]
    public required string PlainToken { get; init; }

    /// <summary>
    /// 计算签名使用时间戳
    /// </summary>
    [JsonPropertyName("event_ts")]
    [Required]
    public required string EventTs { get; init; }
}

/// <summary>
/// 消息事件输入参数
/// </summary>
public record QQBotMessageInputDto
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("id")]
    [Required]
    public required string Id { get; init; }

    /// <summary>
    /// 频道ID
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; init; }

    /// <summary>
    /// 子频道ID
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; init; }

    /// <summary>
    /// 群ID
    /// </summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; init; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    [Required]
    public required string Content { get; init; }

    /// <summary>
    /// 发送时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; init; }

    /// <summary>
    /// 作者信息
    /// </summary>
    [JsonPropertyName("author")]
    public QQBotAuthorInputDto? Author { get; init; }

    /// <summary>
    /// 成员信息
    /// </summary>
    [JsonPropertyName("member")]
    public QQBotMemberInputDto? Member { get; init; }

    /// <summary>
    /// @用户列表
    /// </summary>
    [JsonPropertyName("mentions")]
    public List<QQBotMentionInputDto>? Mentions { get; init; }

    /// <summary>
    /// 附件列表
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<QQBotAttachmentInputDto>? Attachments { get; init; }
}

/// <summary>
/// 消息作者输入参数
/// </summary>
public record QQBotAuthorInputDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>
    /// 用户OpenID
    /// </summary>
    [JsonPropertyName("user_openid")]
    public string? UserOpenId { get; init; }

    /// <summary>
    /// 成员OpenID
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string? MemberOpenId { get; init; }

    /// <summary>
    /// 头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }

    /// <summary>
    /// 是否是机器人
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; init; }
}

/// <summary>
/// 成员信息输入参数
/// </summary>
public record QQBotMemberInputDto
{
    /// <summary>
    /// 昵称
    /// </summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; init; }

    /// <summary>
    /// 角色列表
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; init; }
}

/// <summary>
/// @用户输入参数
/// </summary>
public record QQBotMentionInputDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }
}

/// <summary>
/// 附件输入参数
/// </summary>
public record QQBotAttachmentInputDto
{
    /// <summary>
    /// 附件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; init; }

    /// <summary>
    /// 内容类型
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; init; }

    /// <summary>
    /// 下载地址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}
