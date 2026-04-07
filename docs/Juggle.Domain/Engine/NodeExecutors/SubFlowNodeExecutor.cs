namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// SUB_FLOW 子流程节点执行器。
/// 执行逻辑：
///   1. 从当前 FlowContext 中按 inputMappings 构建子流程入参
///   2. 调用 FlowEngine 递归执行指定子流程
///   3. 按 outputMappings 将子流程输出变量写回当前 FlowContext
/// 注意：子流程的节点日志会追加到当前上下文的 NodeLogs 中（带前缀标识）
/// </summary>
public class SubFlowNodeExecutor : INodeExecutor
{
    private readonly Func<string, Task<string?>> _flowContentLoader;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Dictionary<string, DataSourceInfo> _dataSources;
    private readonly Dictionary<string, string?> _staticVariables;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="flowContentLoader">根据 flowKey 获取最新发布版本 flowContent 的委托</param>
    /// <param name="httpClientFactory">HTTP 客户端工厂</param>
    /// <param name="dataSources">数据源信息字典</param>
    /// <param name="staticVariables">静态变量快照</param>
    public SubFlowNodeExecutor(
        Func<string, Task<string?>> flowContentLoader,
        IHttpClientFactory httpClientFactory,
        Dictionary<string, DataSourceInfo> dataSources,
        Dictionary<string, string?> staticVariables)
    {
        _flowContentLoader = flowContentLoader;
        _httpClientFactory = httpClientFactory;
        _dataSources       = dataSources;
        _staticVariables   = staticVariables;
    }

    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.SubFlowConfig
            ?? throw new InvalidOperationException($"SUB_FLOW node [{node.Key}] 未配置 subFlowConfig");

        if (string.IsNullOrWhiteSpace(cfg.SubFlowKey))
            throw new InvalidOperationException($"SUB_FLOW node [{node.Key}] subFlowKey 不能为空");

        // 1. 加载子流程内容
        var subContent = await _flowContentLoader(cfg.SubFlowKey);
        if (string.IsNullOrEmpty(subContent) || subContent == "[]")
            throw new InvalidOperationException($"SUB_FLOW node [{node.Key}] 找不到子流程 [{cfg.SubFlowKey}] 或流程内容为空");

        // 2. 构建子流程入参
        var subInput = new Dictionary<string, object?>();
        foreach (var mapping in cfg.InputMappings)
        {
            var val = mapping.SourceType?.ToUpper() == "CONSTANT"
                ? (object?)mapping.Source
                : context.GetVariable(mapping.Source);
            subInput[mapping.Target] = val;
        }

        // 3. 执行子流程（传入相同的静态变量副本以支持 STATIC 操作）
        var subEngine = new FlowEngine(_httpClientFactory, _dataSources, new Dictionary<string, string?>(_staticVariables, StringComparer.OrdinalIgnoreCase));
        var subResult = await subEngine.ExecuteAsync(subContent, subInput, cfg.SubFlowKey);

        if (!subResult.Success)
            throw new InvalidOperationException($"子流程 [{cfg.SubFlowKey}] 执行失败: {subResult.ErrorMessage}");

        // 4. 将子流程节点日志追加到当前上下文（带前缀）
        if (subResult.Context?.NodeLogs != null)
        {
            foreach (var nl in subResult.Context.NodeLogs)
            {
                nl.NodeLabel = $"[子流程:{cfg.SubFlowKey}] {nl.NodeLabel}";
                context.NodeLogs.Add(nl);
            }
        }

        // 5. 将子流程的静态变量修改同步回当前上下文
        if (subResult.Context != null)
        {
            foreach (var code in subResult.Context.ModifiedStaticVarCodes)
            {
                if (subResult.Context.StaticVariables.TryGetValue(code, out var val))
                {
                    context.SetStaticVariable(code, val);
                }
            }
        }

        // 6. 按 outputMappings 将子流程输出写回当前变量上下文
        foreach (var mapping in cfg.OutputMappings)
        {
            if (subResult.OutputData.TryGetValue(mapping.Source, out var outVal))
                context.SetVariable(mapping.Target, outVal);
            else if (subResult.Context?.Variables.TryGetValue(mapping.Source, out var varVal) == true)
                context.SetVariable(mapping.Target, varVal);
        }

        return node.Outgoings.FirstOrDefault();
    }
}
