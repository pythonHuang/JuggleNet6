namespace Juggle.Domain.Entities;

/// <summary>角色（TenantId=null 表示全局角色，跨租户共享）</summary>
public class RoleEntity : BaseEntity
{
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
    // TenantId 继承自 BaseEntity，null 表示全局角色
}
