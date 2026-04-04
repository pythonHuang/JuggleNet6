namespace Juggle.Domain.Entities;

/// <summary>租户</summary>
public class TenantEntity : BaseEntity
{
    public string TenantName { get; set; } = "";
    public string? TenantCode { get; set; }
    public string? Remark { get; set; }
    public int Status { get; set; } = 1;  // 1启用 0禁用
    /// <summary>过期时间（null 表示永不过期），过期后自动禁用</summary>
    public DateTime? ExpiredAt { get; set; }
    /// <summary>租户拥有的菜单权限（JSON 数组），如 ["flow/define","flow/log"]</summary>
    public string MenuKeys { get; set; } = "[]";
}
