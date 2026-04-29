namespace Juggle.Domain.Entities;

/// <summary>
/// 角色实体
/// 用于权限控制，TenantId=null 表示全局角色（跨租户共享）
/// </summary>
/// <remarks>
/// 角色分类：
/// - 全局角色：TenantId=null，由系统管理员维护，所有租户可见
/// - 租户角色：TenantId=有值，仅所属租户可见
/// </remarks>
public class RoleEntity : BaseEntity
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = "";

    /// <summary>
    /// 角色编码，用于程序逻辑判断（如 "admin" 表示超级管理员）
    /// </summary>
    public string? RoleCode { get; set; }

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Remark { get; set; }

    // TenantId 继承自 BaseEntity，null 表示全局角色（跨租户共享）
}
