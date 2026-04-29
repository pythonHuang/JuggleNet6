namespace Juggle.Domain.Entities;

/// <summary>
/// 审计日志实体
/// 记录用户的关键操作行为，用于安全审计和操作追溯
/// </summary>
public class AuditLogEntity : BaseEntity
{
    /// <summary>
    /// 操作模块
    /// 例如：tenant（租户）、role（角色）、user（用户）、flow（流程）等
    /// </summary>
    public string Module { get; set; } = "";

    /// <summary>
    /// 操作类型
    /// 例如：add（新增）、update（修改）、delete（删除）
    /// </summary>
    public string ActionType { get; set; } = "";

    /// <summary>
    /// 操作目标 ID（被操作记录的主键）
    /// </summary>
    public long TargetId { get; set; }

    /// <summary>
    /// 变更内容摘要（JSON 格式）
    /// 记录修改前后的字段值变化
    /// </summary>
    public string ChangeContent { get; set; } = "{}";

    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作人用户 ID
    /// </summary>
    public long? OperatorId { get; set; }

    /// <summary>
    /// 操作人所属租户 ID
    /// 等同于 TenantId，保留用于兼容旧查询
    /// </summary>
    public long? OperatorTenantId { get; set; }

    // TenantId 继承自 BaseEntity
}
