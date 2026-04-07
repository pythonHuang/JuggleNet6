namespace Juggle.Domain.Entities;

/// <summary>流程节点执行明细日志（每个节点执行记录一条）</summary>
public class FlowNodeLogEntity : BaseEntity
{
    /// <summary>关联的主日志ID</summary>
    public long? FlowLogId { get; set; }
    /// <summary>节点Key</summary>
    public string? NodeKey { get; set; }
    /// <summary>节点标签/名称</summary>
    public string? NodeLabel { get; set; }
    /// <summary>节点类型：START/END/METHOD/CONDITION/ASSIGN/CODE/MYSQL/MERGE</summary>
    public string? NodeType { get; set; }
    /// <summary>执行顺序（从1开始）</summary>
    public int? SeqNo { get; set; }
    /// <summary>执行状态：SUCCESS / FAILED / SKIPPED</summary>
    public string? Status { get; set; }
    /// <summary>开始时间</summary>
    public string? StartTime { get; set; }
    /// <summary>结束时间</summary>
    public string? EndTime { get; set; }
    /// <summary>耗时（毫秒）</summary>
    public long? CostMs { get; set; }
    /// <summary>节点输入（变量快照 JSON）</summary>
    public string? InputSnapshot { get; set; }
    /// <summary>节点输出（变量快照 JSON）</summary>
    public string? OutputSnapshot { get; set; }
    /// <summary>日志详情（如SQL、HTTP请求等）</summary>
    public string? Detail { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
}
