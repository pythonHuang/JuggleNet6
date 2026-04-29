namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增流程定义请求模型
/// </summary>
public class FlowDefinitionAddRequest
{
    /// <summary>
    /// 流程名称
    /// </summary>
    public string FlowName { get; set; } = "";

    /// <summary>
    /// 流程描述
    /// </summary>
    public string? FlowDesc { get; set; }

    /// <summary>
    /// 流程类型：sync（同步）/ async（异步）
    /// </summary>
    public string FlowType { get; set; } = "sync";

    /// <summary>
    /// 分组名称（用于流程分类管理）
    /// </summary>
    public string? GroupName { get; set; }
}