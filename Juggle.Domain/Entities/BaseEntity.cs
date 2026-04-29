namespace Juggle.Domain.Entities;

/// <summary>
/// 所有业务实体的基类
/// 提供公共字段，包括主键、软删除标记、租户隔离字段、审计字段
/// </summary>
/// <remarks>
/// TenantId 字段说明：
/// - TenantId = null：表示全局数据，不受租户隔离（如官方套件、全局角色）
/// - TenantId 有值时：DbContext 的全局查询过滤器会自动按当前请求租户隔离数据
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 软删除标记：0-未删除，1-已删除
    /// 已删除的记录在查询时会被自动过滤
    /// </summary>
    public int Deleted { get; set; } = 0;

    /// <summary>
    /// 所属租户 ID
    /// - null = 全局数据（不隔离）
    /// - 有值 = 租户私有数据
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 创建时间（ISO 8601 格式字符串）
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// 创建人 ID
    /// </summary>
    public long? CreatedBy { get; set; }

    /// <summary>
    /// 最后更新时间（ISO 8601 格式字符串）
    /// </summary>
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// 最后更新人 ID
    /// </summary>
    public long? UpdatedBy { get; set; }
}
