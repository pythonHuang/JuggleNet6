namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程执行日志分页查询请求
/// </summary>
public class FlowLogPageRequest : PageRequest
{
    /// <summary>
    /// 流程标识 Key（可选）
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 执行状态（可选）
    /// 如：success、failed、running
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 开始日期（可选）
    /// 格式：yyyy-MM-dd
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// 结束日期（可选）
    /// 格式：yyyy-MM-dd
    /// </summary>
    public string? EndDate { get; set; }
}