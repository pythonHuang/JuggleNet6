namespace Juggle.Application.Models.Request;

public class TokenPermissionSaveRequest
{
    public string PermissionType { get; set; } = "";  // FLOW / API
    public string ResourceKey { get; set; } = "";     // flowKey / methodCode
    public string? ResourceName { get; set; }
}