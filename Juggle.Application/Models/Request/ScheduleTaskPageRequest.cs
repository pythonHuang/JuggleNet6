namespace Juggle.Application.Models.Request;

/// <summary>
/// 定时任务分页查询请求
/// </summary>
public class ScheduleTaskPageRequest : PageRequest
{
    /// <summary>
    /// 流程标识 Key（可选）
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 任务状态（可选）
    /// 0-禁用；1-启用
    /// </summary>
    public int? Status { get; set; }
}