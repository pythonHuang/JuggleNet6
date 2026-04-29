namespace Juggle.Domain.Entities;

/// <summary>
/// 流程执行日志实体
/// 每次流程触发时记录一条主日志，与 FlowNodeLogEntity 形成主从关系
/// </summary>
public class FlowLogEntity : BaseEntity
{
    /// <summary>
    /// 流程唯一标识 Key
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 流程名称（冗余存储，便于独立查询）
    /// </summary>
    public string? FlowName { get; set; }

    /// <summary>
    /// 版本号（如 "v1"、"v2" 等）
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 触发方式：
    /// - debug：调试触发
    /// - open：开放接口触发
    /// - schedule：定时任务触发
    /// - open_async：异步开放接口触发
    /// </summary>
    public string? TriggerType { get; set; }

    /// <summary>
    /// 执行状态：
    /// - SUCCESS：执行成功
    /// - FAILED：执行失败
    /// - RUNNING：执行中（仅异步流程预写入）
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 开始时间（ISO 8601 格式字符串）
    /// </summary>
    public string? StartTime { get; set; }

    /// <summary>
    /// 结束时间（ISO 8601 格式字符串）
    /// </summary>
    public string? EndTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long? CostMs { get; set; }

    /// <summary>
    /// 错误信息（执行失败时记录具体异常信息）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 输入参数 JSON（流程执行时的入参快照）
    /// </summary>
    public string? InputJson { get; set; }

    /// <summary>
    /// 输出结果 JSON（以 output_ 开头的变量值）
    /// </summary>
    public string? OutputJson { get; set; }
}
