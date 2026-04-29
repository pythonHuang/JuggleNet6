namespace Juggle.Domain.Entities;

/// <summary>
/// 流程节点执行明细日志实体
/// 记录每个节点的执行详情，用于问题排查和性能分析
/// 与 FlowLogEntity 形成主从关系（FlowLogId 关联）
/// </summary>
public class FlowNodeLogEntity : BaseEntity
{
    /// <summary>
    /// 关联的主日志 ID（FlowLogEntity.Id）
    /// </summary>
    public long? FlowLogId { get; set; }

    /// <summary>
    /// 节点唯一标识 Key
    /// </summary>
    public string? NodeKey { get; set; }

    /// <summary>
    /// 节点显示名称/标签
    /// </summary>
    public string? NodeLabel { get; set; }

    /// <summary>
    /// 节点类型：
    /// - START：开始节点
    /// - END：结束节点
    /// - METHOD：HTTP/WebService 调用节点
    /// - CONDITION：条件分支节点
    /// - ASSIGN：赋值节点
    /// - CODE：代码执行节点
    /// - MYSQL：数据库操作节点
    /// - MERGE：分支汇合节点
    /// - SUB_FLOW：子流程调用节点
    /// - LOOP：循环节点
    /// - DELAY：延迟节点
    /// - PARALLEL：并行节点
    /// - NOTIFY：通知节点
    /// </summary>
    public string? NodeType { get; set; }

    /// <summary>
    /// 执行顺序号（从 1 开始，按执行时间递增）
    /// </summary>
    public int? SeqNo { get; set; }

    /// <summary>
    /// 执行状态：
    /// - RUNNING：执行中
    /// - SUCCESS：执行成功
    /// - FAILED：执行失败
    /// - SKIPPED：跳过（未执行）
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 节点开始执行时间（ISO 8601 格式字符串）
    /// </summary>
    public string? StartTime { get; set; }

    /// <summary>
    /// 节点结束执行时间（ISO 8601 格式字符串）
    /// </summary>
    public string? EndTime { get; set; }

    /// <summary>
    /// 节点执行耗时（毫秒）
    /// </summary>
    public long? CostMs { get; set; }

    /// <summary>
    /// 节点执行前的变量快照（JSON 格式）
    /// </summary>
    public string? InputSnapshot { get; set; }

    /// <summary>
    /// 节点执行后的变量快照（JSON 格式）
    /// </summary>
    public string? OutputSnapshot { get; set; }

    /// <summary>
    /// 日志详情（如 SQL 语句、HTTP 请求信息、条件表达式结果等）
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// 错误信息（节点执行失败时记录具体异常）
    /// </summary>
    public string? ErrorMessage { get; set; }
}
