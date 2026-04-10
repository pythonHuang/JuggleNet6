namespace Juggle.Domain.Entities;

/// <summary>
/// 所有业务实体的基类。
/// TenantId = null 表示全局数据（不受租户隔离，如官方套件、全局角色）；
/// TenantId 有值时，DbContext 的全局查询过滤器会自动按当前请求租户隔离。
/// </summary>
public abstract class BaseEntity
{
    public long Id { get; set; }
    public int Deleted { get; set; } = 0;
    /// <summary>所属租户ID（null = 全局/不隔离）</summary>
    public long? TenantId { get; set; }
    public string? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public string? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
