namespace Juggle.Domain.Entities;

/// <summary>
/// 定时任务实体：给已发布流程绑定 Cron 表达式，定时自动触发。
/// </summary>
public class ScheduleTaskEntity : BaseEntity
{
    public string? FlowKey { get; set; }       // 关联的流程 Key
    public string? FlowName { get; set; }       // 流程名称（冗余，方便展示）
    public string? CronExpression { get; set; } // Cron 表达式，如 "0 */5 * * * *"
    public string? InputJson { get; set; }      // 触发时的固定入参 JSON
    public int Status { get; set; } = 0;        // 0:暂停 1:启用
    public DateTime? LastRunTime { get; set; }  // 上次执行时间
    public string? LastRunStatus { get; set; }  // 上次执行状态 SUCCESS/FAILED
    public DateTime? NextRunTime { get; set; }  // 下次执行时间（预计算）
    public int RunCount { get; set; } = 0;      // 累计执行次数
}
