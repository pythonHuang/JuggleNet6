namespace Juggle.Domain.Entities;

/// <summary>
/// 流程版本实体
/// 存储流程部署时的版本快照，用于版本管理和回滚
/// </summary>
public class FlowVersionEntity : BaseEntity
{
    /// <summary>
    /// 关联的流程信息 ID
    /// </summary>
    public long? FlowInfoId { get; set; }

    /// <summary>
    /// 流程唯一标识 Key
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 版本号
    /// 格式为 v1、v2、v3 等
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 流程节点配置 JSON（部署时的快照）
    /// 与 FlowDefinitionEntity.FlowContent 保持同步
    /// </summary>
    public string? FlowContent { get; set; }

    /// <summary>
    /// 版本状态：0-禁用，1-启用
    /// 只能有一个启用的版本
    /// </summary>
    public int Status { get; set; } = 1;
}
