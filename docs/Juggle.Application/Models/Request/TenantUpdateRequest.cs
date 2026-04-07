namespace Juggle.Application.Models.Request;

public class TenantUpdateRequest
{
    public long Id { get; set; }
    public string TenantName { get; set; } = "";
    public string? TenantCode { get; set; }
    public string? Remark { get; set; }
    public int Status { get; set; }
    /// <summary>过期时间（null 表示永不过期）</summary>
    public DateTime? ExpiredAt { get; set; }
    /// <summary>租户菜单权限列表</summary>
    public List<string> MenuKeys { get; set; } = new();
    /// <summary>关联用户ID列表</summary>
    public List<long> UserIds { get; set; } = new();
}