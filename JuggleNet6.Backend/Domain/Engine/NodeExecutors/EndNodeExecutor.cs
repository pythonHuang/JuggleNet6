namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

public class EndNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        // END 节点，返回 null 表示流程结束
        return Task.FromResult<string?>(null);
    }
}
