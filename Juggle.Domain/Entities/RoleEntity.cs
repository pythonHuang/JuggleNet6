namespace Juggle.Domain.Entities;

/// <summary>角色</summary>
public class RoleEntity : BaseEntity
{
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
    /// <summary>角色所属租户（null 表示全局角色，跨租户共享）</summary>
    public long? TenantId { get; set; }
}
