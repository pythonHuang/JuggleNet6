namespace Juggle.Domain.Entities;

/// <summary>
/// 流程定义实体
/// 存储流程的基本信息和节点配置 JSON
/// </summary>
public class FlowDefinitionEntity : BaseEntity
{
    /// <summary>
    /// 流程唯一标识 Key
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 流程名称
    /// </summary>
    public string? FlowName { get; set; }

    /// <summary>
    /// 流程描述
    /// </summary>
    public string? FlowDesc { get; set; }

    /// <summary>
    /// 流程节点 JSON 配置
    /// 包含完整的节点定义、连接线、变量映射等信息
    /// </summary>
    public string? FlowContent { get; set; }

    /// <summary>
    /// 流程类型：sync（同步）/ async（异步）
    /// </summary>
    public string? FlowType { get; set; }

    /// <summary>
    /// 分组名称，用于流程分类管理
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 状态：0-草稿，1-已部署
    /// </summary>
    public int Status { get; set; } = 0;
}
