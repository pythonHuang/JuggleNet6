namespace Juggle.Domain.Entities;

/// <summary>流程执行日志（每次流程触发记录一条主日志）</summary>
public class FlowLogEntity : BaseEntity
{
    /// <summary>流程Key</summary>
    public string? FlowKey { get; set; }
    /// <summary>流程名称（冗余）</summary>
    public string? FlowName { get; set; }
    /// <summary>版本号</summary>
    public string? Version { get; set; }
    /// <summary>触发方式：debug / open / schedule</summary>
    public string? TriggerType { get; set; }
    /// <summary>执行状态：SUCCESS / FAILED</summary>
    public string? Status { get; set; }
    /// <summary>开始时间</summary>
    public string? StartTime { get; set; }
    /// <summary>结束时间</summary>
    public string? EndTime { get; set; }
    /// <summary>耗时（毫秒）</summary>
    public long? CostMs { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>输入参数 JSON</summary>
    public string? InputJson { get; set; }
    /// <summary>输出结果 JSON</summary>
    public string? OutputJson { get; set; }
}
