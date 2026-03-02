using System.Text.Json.Serialization;

namespace Endfield.Api.Models.QQBot;

#region Webhook 回调模型

/// <summary>
/// Webhook 回调 Payload
/// </summary>
public class QQWebhookPayload
{
    /// <summary>
    /// 事件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 操作码
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// 事件数据
    /// </summary>
    [JsonPropertyName("d")]
    public object? Data { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    [JsonPropertyName("s")]
    public long Sequence { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [JsonPropertyName("t")]
    public string? EventType { get; set; }
}

/// <summary>
/// 回调地址验证请求
/// </summary>
public class QQValidationRequest
{
    /// <summary>
    /// 需要计算签名的字符串
    /// </summary>
    [JsonPropertyName("plain_token")]
    public string PlainToken { get; set; } = null!;

    /// <summary>
    /// 计算签名使用时间戳
    /// </summary>
    [JsonPropertyName("event_ts")]
    public string EventTs { get; set; } = null!;
}

/// <summary>
/// 回调地址验证响应
/// </summary>
public class QQValidationResponse
{
    /// <summary>
    /// 需要计算签名的字符串
    /// </summary>
    [JsonPropertyName("plain_token")]
    public string PlainToken { get; set; } = null!;

    /// <summary>
    /// 签名
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = null!;
}

#endregion

#region 消息事件模型

/// <summary>
/// 消息事件（群/频道AT消息）
/// </summary>
public class QQMessageEvent
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 频道ID（群消息时存在）
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// 子频道ID（群消息时存在）
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// 群ID（群聊消息时存在）
    /// </summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 发送时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = null!;

    /// <summary>
    /// 作者信息
    /// </summary>
    [JsonPropertyName("author")]
    public QQMessageAuthor Author { get; set; } = null!;

    /// <summary>
    /// 成员信息（群消息时存在）
    /// </summary>
    [JsonPropertyName("member")]
    public QQGuildMember? Member { get; set; }

    /// <summary>
    /// 消息中@的用户列表
    /// </summary>
    [JsonPropertyName("mentions")]
    public List<QQMessageMention>? Mentions { get; set; }

    /// <summary>
    /// 附件列表
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<QQAttachment>? Attachments { get; set; }
}

/// <summary>
/// 私信消息事件
/// </summary>
public class QQC2CMessageEvent
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 发送时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = null!;

    /// <summary>
    /// 作者信息
    /// </summary>
    [JsonPropertyName("author")]
    public QQMessageAuthor Author { get; set; } = null!;

    /// <summary>
    /// 附件列表
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<QQAttachment>? Attachments { get; set; }
}

/// <summary>
/// 消息作者
/// </summary>
public class QQMessageAuthor
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 用户OpenID（私聊消息时）
    /// </summary>
    [JsonPropertyName("user_openid")]
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 成员OpenID（群聊消息时）
    /// </summary>
    [JsonPropertyName("member_openid")]
    public string? MemberOpenId { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 是否是机器人
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }
}

/// <summary>
/// 频道成员
/// </summary>
public class QQGuildMember
{
    /// <summary>
    /// 昵称
    /// </summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }
}

/// <summary>
/// 消息中@的用户
/// </summary>
public class QQMessageMention
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;
}

/// <summary>
/// 附件
/// </summary>
public class QQAttachment
{
    /// <summary>
    /// 附件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

#endregion

#region API 请求模型

/// <summary>
/// 发送群消息请求
/// </summary>
public class QQSendGroupMessageRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 引用消息ID
    /// </summary>
    [JsonPropertyName("msg_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 图片URL
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Markdown模板
    /// </summary>
    [JsonPropertyName("markdown")]
    public QQMarkdown? Markdown { get; set; }
}

/// <summary>
/// 发送私信请求
/// </summary>
public class QQSendC2CMessageRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 引用消息ID
    /// </summary>
    [JsonPropertyName("msg_id")]
    public string? MessageId { get; set; }
}

/// <summary>
/// Markdown 模板
/// </summary>
public class QQMarkdown
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [JsonPropertyName("custom_template_id")]
    public string? CustomTemplateId { get; set; }

    /// <summary>
    /// 参数列表
    /// </summary>
    [JsonPropertyName("params")]
    public List<QQMarkdownParam>? Params { get; set; }

    /// <summary>
    /// 原始Markdown内容
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// Markdown 参数
/// </summary>
public class QQMarkdownParam
{
    /// <summary>
    /// 参数键
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// 参数值列表
    /// </summary>
    [JsonPropertyName("values")]
    public List<string> Values { get; set; } = new();
}

#endregion

/// <summary>
/// Webhook 操作码
/// </summary>
public static class QQWebhookOpCodes
{
    /// <summary>
    /// 事件分发
    /// </summary>
    public const int Dispatch = 0;

    /// <summary>
    /// 心跳
    /// </summary>
    public const int Heartbeat = 1;

    /// <summary>
    /// 鉴权
    /// </summary>
    public const int Identify = 2;

    /// <summary>
    /// 恢复连接
    /// </summary>
    public const int Resume = 6;

    /// <summary>
    /// 重连
    /// </summary>
    public const int Reconnect = 7;

    /// <summary>
    /// 无效会话
    /// </summary>
    public const int InvalidSession = 9;

    /// <summary>
    /// Hello
    /// </summary>
    public const int Hello = 10;

    /// <summary>
    /// 心跳ACK
    /// </summary>
    public const int HeartbeatAck = 11;

    /// <summary>
    /// HTTP回调ACK
    /// </summary>
    public const int HttpCallbackAck = 12;

    /// <summary>
    /// 回调地址验证
    /// </summary>
    public const int Validation = 13;
}

/// <summary>
/// 事件类型常量
/// </summary>
public static class QQEventTypes
{
    /// <summary>
    /// 准备就绪
    /// </summary>
    public const string Ready = "READY";

    /// <summary>
    /// 频道@消息（公域）
    /// </summary>
    public const string AtMessageCreate = "AT_MESSAGE_CREATE";

    /// <summary>
    /// 私信消息
    /// </summary>
    public const string C2CMessageCreate = "C2C_MESSAGE_CREATE";

    /// <summary>
    /// 群@消息（私域）
    /// </summary>
    public const string GroupAtMessageCreate = "GROUP_AT_MESSAGE_CREATE";

    /// <summary>
    /// 好友消息
    /// </summary>
    public const string FriendAdd = "FRIEND_ADD";
}
