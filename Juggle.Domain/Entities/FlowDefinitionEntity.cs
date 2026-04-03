namespace Juggle.Domain.Entities;

public class FlowDefinitionEntity : BaseEntity
{
    public string? FlowKey { get; set; }       // 流程唯一key
    public string? FlowName { get; set; }
    public string? FlowDesc { get; set; }
    public string? FlowContent { get; set; }   // 流程节点JSON
    public string? FlowType { get; set; }      // sync/async
    public string? GroupName { get; set; }     // 分组名称
    public int Status { get; set; } = 0;       // 0:草稿 1:已部署
}
