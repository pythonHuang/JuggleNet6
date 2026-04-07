namespace Juggle.Application.Models.Request;

public class FlowLogPageRequest : PageRequest
{
    public string? FlowKey   { get; set; }
    public string? Status    { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate   { get; set; }
}