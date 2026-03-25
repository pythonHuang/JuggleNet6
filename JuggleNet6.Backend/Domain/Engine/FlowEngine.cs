using System.Text.Json;
using JuggleNet6.Backend.Domain.Engine.NodeExecutors;

namespace JuggleNet6.Backend.Domain.Engine;

/// <summary>
/// 流程执行引擎：解析流程 JSON，按节点拓扑顺序执行，维护变量上下文
/// 支持节点类型：START / END / METHOD / CONDITION / ASSIGN / CODE / MYSQL
/// </summary>
public class FlowEngine
{
    private readonly IHttpClientFactory _httpClientFactory;
    /// <summary>数据源名称 → 连接字符串映射（由 FlowDefinitionController 注入）</summary>
    private readonly Dictionary<string, string> _dataSourceConnStrings;

    public FlowEngine(IHttpClientFactory httpClientFactory,
                      Dictionary<string, string>? dataSourceConnStrings = null)
    {
        _httpClientFactory = httpClientFactory;
        _dataSourceConnStrings = dataSourceConnStrings ?? new();
    }

    public async Task<FlowResult> ExecuteAsync(
        string flowContent,
        Dictionary<string, object?> inputParams,
        string flowKey = "",
        string version = "")
    {
        var result = new FlowResult { Success = true };

        // 1. 解析流程节点
        List<FlowNode>? nodes;
        try
        {
            nodes = JsonSerializer.Deserialize<List<FlowNode>>(flowContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new FlowResult { Success = false, ErrorMessage = $"流程内容解析失败: {ex.Message}" };
        }

        if (nodes == null || nodes.Count == 0)
            return new FlowResult { Success = false, ErrorMessage = "流程内容为空" };

        // 2. 建立节点索引
        var nodeMap = nodes.ToDictionary(n => n.Key, n => n);

        // 3. 初始化上下文，写入输入变量
        var context = new FlowContext { FlowKey = flowKey, Version = version };
        foreach (var kv in inputParams)
            context.SetVariable(kv.Key, kv.Value);

        // 4. 找到 START 节点
        var startNode = nodes.FirstOrDefault(n => n.ElementType == "START");
        if (startNode == null)
            return new FlowResult { Success = false, ErrorMessage = "找不到 START 节点" };

        // 5. 按拓扑顺序执行
        try
        {
            var currentKey = startNode.Key;
            var visited = new HashSet<string>();
            const int maxSteps = 1000; // 防死循环
            var steps = 0;

            while (!string.IsNullOrEmpty(currentKey) && steps++ < maxSteps)
            {
                if (!nodeMap.TryGetValue(currentKey, out var currentNode))
                    break;

                // 检测死循环（CONDITION 节点不计入 visited）
                if (currentNode.ElementType != "CONDITION" && !visited.Add(currentKey))
                    return new FlowResult { Success = false, ErrorMessage = $"检测到死循环，节点: {currentKey}" };

                INodeExecutor executor = currentNode.ElementType switch
                {
                    "START"     => new StartNodeExecutor(),
                    "END"       => new EndNodeExecutor(),
                    "METHOD"    => new MethodNodeExecutor(_httpClientFactory),
                    "CONDITION" => new ConditionNodeExecutor(),
                    "ASSIGN"    => new AssignNodeExecutor(),
                    "CODE"      => new CodeNodeExecutor(),
                    "MYSQL"     => new MysqlNodeExecutor(_dataSourceConnStrings),
                    _ => throw new InvalidOperationException($"未知节点类型: {currentNode.ElementType}")
                };

                var nextKey = await executor.ExecuteAsync(currentNode, context);

                if (currentNode.ElementType == "END" || nextKey == null)
                    break;

                currentKey = nextKey;
            }
        }
        catch (Exception ex)
        {
            return new FlowResult { Success = false, ErrorMessage = ex.Message };
        }

        // 6. 收集所有输出变量（以 output_ 开头的变量）
        foreach (var kv in context.Variables)
        {
            if (kv.Key.StartsWith("output_"))
                result.OutputData[kv.Key] = kv.Value;
        }

        return result;
    }
}
