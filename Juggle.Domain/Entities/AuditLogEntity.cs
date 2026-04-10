namespace Juggle.Domain.Entities;

/// <summary>审计日志</summary>
public class AuditLogEntity : BaseEntity
{
    /// <summary>操作模块：tenant / role / user</summary>
    public string Module { get; set; } = "";
    /// <summary>操作类型：add / update / delete</summary>
    public string ActionType { get; set; } = "";
    /// <summary>操作目标ID</summary>
    public long TargetId { get; set; }
    /// <summary>变更内容摘要（JSON）</summary>
    public string ChangeContent { get; set; } = "{}";
    /// <summary>操作人用户名</summary>
    public string? OperatorName { get; set; }
    /// <summary>操作人用户ID</summary>
    public long? OperatorId { get; set; }
    /// <summary>操作人租户ID（等同于 TenantId，保留用于兼容旧查询）</summary>
    public long? OperatorTenantId { get; set; }
    // TenantId 继承自 BaseEntity
}
