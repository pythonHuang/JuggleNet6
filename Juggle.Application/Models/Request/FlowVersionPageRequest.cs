namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程版本分页查询请求
/// </summary>
public class FlowVersionPageRequest : PageRequest
{
    /// <summary>
    /// 流程标识 Key（可选）
    /// 用于筛选指定流程的所有版本
    /// </summary>
    public string? FlowKey { get; set; }
}