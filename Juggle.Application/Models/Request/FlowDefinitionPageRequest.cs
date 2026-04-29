namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程定义分页查询请求
/// </summary>
public class FlowDefinitionPageRequest : PageRequest
{
    /// <summary>
    /// 流程名称（可选）
    /// 模糊匹配
    /// </summary>
    public string? FlowName { get; set; }

    /// <summary>
    /// 分组名称（可选）
    /// </summary>
    public string? GroupName { get; set; }
}