namespace Juggle.Application.Models.Request;

/// <summary>
/// 定时任务更新请求
/// </summary>
public class ScheduleTaskUpdateRequest : ScheduleTaskAddRequest
{
    /// <summary>
    /// 定时任务 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 任务状态
    /// 0-禁用；1-启用
    /// </summary>
    public int Status { get; set; }
}