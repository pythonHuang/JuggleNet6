namespace Juggle.Application.Models.Request;

/// <summary>
/// 角色更新请求
/// </summary>
public class RoleUpdateRequest
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = "";

    /// <summary>
    /// 角色编码（可选）
    /// </summary>
    public string? RoleCode { get; set; }

    /// <summary>
    /// 备注（可选）
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 租户 ID（可选）
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 菜单权限标识列表
    /// </summary>
    public List<string> MenuKeys { get; set; } = new();
}