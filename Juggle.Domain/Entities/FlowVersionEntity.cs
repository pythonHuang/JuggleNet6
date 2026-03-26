namespace Juggle.Domain.Entities;

public class FlowVersionEntity : BaseEntity
{
    public long? FlowInfoId { get; set; }
    public string? FlowKey { get; set; }
    public string? Version { get; set; }     // v1, v2, ...
    public string? FlowContent { get; set; } // 部署时快照的流程JSON
    /// <summary>0:禁用 1:启用</summary>
    public int Status { get; set; } = 1;
}
