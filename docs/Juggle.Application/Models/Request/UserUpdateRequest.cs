namespace Juggle.Application.Models.Request;

public class UserUpdateRequest
{
    public long Id { get; set; }
    public string UserName { get; set; } = "";
    public long? RoleId { get; set; }
    public long? TenantId { get; set; }
}