namespace Juggle.Domain.Entities;

/// <summary>
/// 定时任务实体
/// 给已发布的流程绑定 Cron 表达式，实现定时自动触发执行
/// </summary>
public class ScheduleTaskEntity : BaseEntity
{
    /// <summary>
    /// 关联的流程唯一标识 Key
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 流程名称（冗余存储，便于展示）
    /// </summary>
    public string? FlowName { get; set; }

    /// <summary>
    /// Cron 表达式
    /// 用于定义执行时间规则，格式为 6 位（秒 分 时 日 月 周）
    /// 例如：
    /// - "0 */5 * * * *"：每 5 分钟执行一次
    /// - "0 0 * * * *"：每小时整点执行
    /// - "0 0 0 * * *"：每天凌晨执行
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 触发时的固定入参 JSON
    /// 定时执行时使用此 JSON 作为流程输入参数
    /// </summary>
    public string? InputJson { get; set; }

    /// <summary>
    /// 任务状态：0-暂停，1-启用
    /// </summary>
    public int Status { get; set; } = 0;

    /// <summary>
    /// 上次执行时间
    /// </summary>
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 上次执行状态：SUCCESS / FAILED
    /// </summary>
    public string? LastRunStatus { get; set; }

    /// <summary>
    /// 下次执行时间（由调度器预计算）
    /// </summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 累计执行次数
    /// </summary>
    public int RunCount { get; set; } = 0;
}
