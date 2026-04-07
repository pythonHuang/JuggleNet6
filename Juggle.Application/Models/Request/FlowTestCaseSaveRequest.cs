namespace Juggle.Application.Models.Request;

public class FlowTestCaseSaveRequest
{
    public long? Id { get; set; }
    public string FlowKey { get; set; } = "";
    public string CaseName { get; set; } = "";
    public string? InputJson { get; set; }
    public string? AssertJson { get; set; }
    public string? Remark { get; set; }
}