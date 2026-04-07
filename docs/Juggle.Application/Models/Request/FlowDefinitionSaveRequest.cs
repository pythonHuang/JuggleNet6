namespace Juggle.Application.Models.Request;

public class FlowDefinitionSaveRequest
{
    public long Id { get; set; }
    public string FlowContent { get; set; } = "[]";
}