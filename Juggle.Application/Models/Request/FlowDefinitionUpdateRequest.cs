namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程定义更新请求
/// </summary>
public class FlowDefinitionUpdateRequest
{
    /// <summary>
    /// 流程定义 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 流程名称
    /// </summary>
    public string FlowName { get; set; } = "";

    /// <summary>
    /// 流程描述（可选）
    /// </summary>
    public string? FlowDesc { get; set; }

    /// <summary>
    /// 流程类型
    /// sync-同步流程；async-异步流程
    /// </summary>
    public string FlowType { get; set; } = "sync";

    /// <summary>
    /// 分组名称（可选）
    /// 用于在界面上按组分类展示流程
    /// </summary>
    public string? GroupName { get; set; }
}