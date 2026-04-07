namespace Juggle.Application.Models.Request;

public class FlowDefinitionPageRequest : PageRequest
{
    public string? FlowName { get; set; }
    public string? GroupName { get; set; }
}