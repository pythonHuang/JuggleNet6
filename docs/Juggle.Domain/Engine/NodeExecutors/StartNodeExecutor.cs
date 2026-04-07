namespace Juggle.Domain.Engine.NodeExecutors;

public class StartNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        // START 节点直接走到下一个节点
        return Task.FromResult(node.Outgoings.FirstOrDefault());
    }
}
