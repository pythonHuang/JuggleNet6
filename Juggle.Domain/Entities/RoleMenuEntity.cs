namespace Juggle.Domain.Entities;

/// <summary>角色菜单权限（多对多中间表）</summary>
public class RoleMenuEntity : BaseEntity
{
    public long RoleId { get; set; }
    public string MenuKey { get; set; } = "";  // 菜单标识，如 "flow/define"
}
