namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// PARALLEL 并行节点执行器：标记一个并行分支起点。
/// 
/// 实际的并行调度逻辑在 FlowEngine 中实现（类似 CONDITION 的特殊处理）。
/// PARALLEL 节点的执行器只负责记录日志和返回 outgoings 供引擎分发。
/// 
/// 工作模式：
/// - ALL_WAIT：所有分支都执行完成后才继续
/// - ANY_FAST：任一分支完成即继续（其余分支继续后台运行）
/// </summary>
public class ParallelNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.ParallelConfig;
        var waitMode = cfg?.WaitMode ?? "ALL_WAIT";

        // PARALLEL 节点不直接决定下一个节点
        // 引擎会根据 outgoings 并行执行各分支
        // 这里返回一个特殊标记，让引擎知道需要并行处理
        return Task.FromResult<string?>($"__PARALLEL__{waitMode}");
    }
}
