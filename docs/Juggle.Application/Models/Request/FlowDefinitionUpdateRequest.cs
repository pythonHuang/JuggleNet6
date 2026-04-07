namespace Juggle.Application.Models.Request;

public class FlowDefinitionUpdateRequest
{
    public long Id { get; set; }
    public string FlowName { get; set; } = "";
    public string? FlowDesc { get; set; }
    public string FlowType { get; set; } = "sync";
    public string? GroupName { get; set; }
}