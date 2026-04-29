using System.Text.Json.Serialization;

namespace Juggle.Domain.Entities;

/// <summary>
/// Webhook 触发配置实体
/// 通过 Webhook 方式从外部系统触发流程执行
/// 访问路径：/open/webhook/{webhookKey}
/// </summary>
public class WebhookEntity : BaseEntity
{
    /// <summary>
    /// Webhook 唯一标识 Key
    /// 用于 URL 路径访问，如 /open/webhook/{webhookKey}
    /// </summary>
    [JsonPropertyName("webhookKey")]
    public string? WebhookKey { get; set; }

    /// <summary>
    /// Webhook 名称（中文描述）
    /// </summary>
    [JsonPropertyName("webhookName")]
    public string? WebhookName { get; set; }

    /// <summary>
    /// 关联的流程唯一标识 Key
    /// </summary>
    [JsonPropertyName("flowKey")]
    public string? FlowKey { get; set; }

    /// <summary>
    /// 关联的流程名称（冗余存储）
    /// </summary>
    [JsonPropertyName("flowName")]
    public string? FlowName { get; set; }

    /// <summary>
    /// 签名密钥
    /// 使用 HMAC-SHA256 进行请求签名验证
    /// 为空则不进行验签
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>
    /// 允许的 HTTP 请求方法：POST / GET / ANY（任意方法）
    /// </summary>
    [JsonPropertyName("allowedMethod")]
    public string AllowedMethod { get; set; } = "POST";

    /// <summary>
    /// 异步执行模式：0-同步（等待执行完成），1-异步（立即返回 logId）
    /// </summary>
    [JsonPropertyName("asyncMode")]
    public int AsyncMode { get; set; } = 0;

    /// <summary>
    /// Webhook 状态：1-启用，0-禁用
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; } = 1;

    /// <summary>
    /// 累计触发次数
    /// </summary>
    [JsonPropertyName("triggerCount")]
    public long TriggerCount { get; set; } = 0;

    /// <summary>
    /// 最后触发时间（ISO 8601 格式字符串）
    /// </summary>
    [JsonPropertyName("lastTriggerTime")]
    public string? LastTriggerTime { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
}
