namespace Juggle.Application.Models.Request;

public class AuditLogPageRequest : PageRequest
{
    public string? Module { get; set; }
    public string? ActionType { get; set; }
}