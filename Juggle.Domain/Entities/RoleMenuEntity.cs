namespace Juggle.Domain.Entities;

/// <summary>
/// 角色菜单权限实体（多对多中间表）
/// 建立角色与菜单的关联关系，用于权限控制
/// </summary>
public class RoleMenuEntity : BaseEntity
{
    /// <summary>
    /// 关联的角色 ID
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 菜单标识 Key
    /// 例如："flow/define"（流程定义）、"flow/log"（流程日志）等
    /// </summary>
    public string MenuKey { get; set; } = "";
}
