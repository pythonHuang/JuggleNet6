namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程定义内容保存请求
/// 用于保存流程的可视化设计内容
/// </summary>
public class FlowDefinitionSaveRequest
{
    /// <summary>
    /// 流程定义 ID（0 表示新增）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 流程设计内容（JSON 格式）
    /// 包含节点、连线等可视化设计信息
    /// </summary>
    public string FlowContent { get; set; } = "[]";
}