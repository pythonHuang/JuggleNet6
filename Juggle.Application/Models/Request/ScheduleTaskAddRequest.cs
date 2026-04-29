namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增定时任务请求模型
/// </summary>
public class ScheduleTaskAddRequest
{
    /// <summary>
    /// 关联的流程 Key
    /// </summary>
    public string FlowKey { get; set; } = "";

    /// <summary>
    /// 流程名称（冗余）
    /// </summary>
    public string? FlowName { get; set; }

    /// <summary>
    /// Cron 表达式
    /// 格式：秒 分 时 日 月 周
    /// 默认值：每 5 分钟执行一次
    /// </summary>
    public string CronExpression { get; set; } = "0 */5 * * * *";

    /// <summary>
    /// 触发时的固定入参 JSON
    /// </summary>
    public string? InputJson { get; set; }
}