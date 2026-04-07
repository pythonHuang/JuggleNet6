namespace Juggle.Domain.Entities;

public class FlowInfoEntity : BaseEntity
{
    public string? FlowKey { get; set; }
    public string? FlowName { get; set; }
    public string? FlowDesc { get; set; }
    public string? FlowType { get; set; }  // sync/async
    public int Status { get; set; } = 1;
}
