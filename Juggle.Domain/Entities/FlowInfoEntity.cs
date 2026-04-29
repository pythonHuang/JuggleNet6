namespace Juggle.Domain.Entities;

/// <summary>
/// 流程信息实体
/// 存储已部署流程的基本信息，与 FlowVersionEntity 形成一对多关系
/// </summary>
public class FlowInfoEntity : BaseEntity
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
    /// 流程类型：
    /// - sync：同步执行
    /// - async：异步执行
    /// </summary>
    public string? FlowType { get; set; }

    /// <summary>
    /// 流程状态：0-禁用，1-启用
    /// </summary>
    public int Status { get; set; } = 1;
}
