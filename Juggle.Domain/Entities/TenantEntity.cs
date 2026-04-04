namespace Juggle.Domain.Entities;

/// <summary>租户</summary>
public class TenantEntity : BaseEntity
{
    public string TenantName { get; set; } = "";
    public string? TenantCode { get; set; }
    public string? Remark { get; set; }
    public int Status { get; set; } = 1;  // 1启用 0禁用
}
