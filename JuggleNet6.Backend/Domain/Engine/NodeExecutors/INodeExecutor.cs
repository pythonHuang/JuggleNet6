namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

public interface INodeExecutor
{
    Task<string?> ExecuteAsync(FlowNode node, FlowContext context);
}
