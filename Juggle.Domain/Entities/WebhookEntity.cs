using System.Text.Json.Serialization;

namespace Juggle.Domain.Entities;

/// <summary>Webhook 触发配置实体</summary>
public class WebhookEntity : BaseEntity
{
    /// <summary>Webhook 唯一标识 Key（用于 URL 路径 /open/webhook/{webhookKey}）</summary>
    [JsonPropertyName("webhookKey")]
    public string? WebhookKey { get; set; }

    /// <summary>Webhook 名称</summary>
    [JsonPropertyName("webhookName")]
    public string? WebhookName { get; set; }

    /// <summary>关联的流程 Key</summary>
    [JsonPropertyName("flowKey")]
    public string? FlowKey { get; set; }

    /// <summary>关联的流程名称（冗余）</summary>
    [JsonPropertyName("flowName")]
    public string? FlowName { get; set; }

    /// <summary>签名密钥（HMAC-SHA256，为空则不验签）</summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>允许的 HTTP 方法：POST / GET / ANY</summary>
    [JsonPropertyName("allowedMethod")]
    public string AllowedMethod { get; set; } = "POST";

    /// <summary>是否异步执行（true=立即返回 logId，false=等待执行完成）</summary>
    [JsonPropertyName("asyncMode")]
    public int AsyncMode { get; set; } = 0;

    /// <summary>状态：1=启用 0=禁用</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; } = 1;

    /// <summary>触发次数统计</summary>
    [JsonPropertyName("triggerCount")]
    public long TriggerCount { get; set; } = 0;

    /// <summary>最后触发时间</summary>
    [JsonPropertyName("lastTriggerTime")]
    public string? LastTriggerTime { get; set; }

    /// <summary>备注</summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
}
