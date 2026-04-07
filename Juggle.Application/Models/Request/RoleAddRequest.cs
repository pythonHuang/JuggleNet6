namespace Juggle.Application.Models.Request;

public class RoleAddRequest
{
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
    public long? TenantId { get; set; }
    public List<string> MenuKeys { get; set; } = new();
}