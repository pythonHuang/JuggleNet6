namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增角色请求模型
/// </summary>
public class RoleAddRequest
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = "";

    /// <summary>
    /// 角色编码
    /// </summary>
    public string? RoleCode { get; set; }

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 所属租户 ID（为 null 表示全局角色）
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 菜单权限 Key 列表
    /// </summary>
    public List<string> MenuKeys { get; set; } = new();
}