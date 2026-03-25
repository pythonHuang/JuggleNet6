using System.Text.Json;
using JuggleNet6.Backend.Domain.Engine.NodeExecutors;

namespace JuggleNet6.Backend.Domain.Engine;

/// <summary>
/// 流程执行引擎：解析流程 JSON，按节点拓扑顺序执行，维护变量上下文
/// 支持节点类型：START / END / METHOD / CONDITION / ASSIGN / CODE / MYSQL(DB) / MERGE
/// </summary>
public class FlowEngine
{
    private readonly IHttpClientFactory _httpClientFactory;
    /// <summary>数据源名称 → DataSourceInfo 映射（由 FlowDefinitionController 注入）</summary>
    private readonly Dictionary<string, DataSourceInfo> _dataSources;

    public FlowEngine(IHttpClientFactory httpClientFactory,
                      Dictionary<string, DataSourceInfo>? dataSources = null)
    {
        _httpClientFactory = httpClientFactory;
        _dataSources = dataSources ?? new();
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

        // 5. 按拓扑顺序执行（支持 CONDITION 多分支 + MERGE 聚合）
        try
        {
            await ExecuteFromNode(startNode.Key, nodeMap, context);
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

    /// <summary>从指定节点开始执行，遇到 MERGE 或 END 时停止并返回下一节点 Key</summary>
    private async Task<string?> ExecuteFromNode(
        string startKey,
        Dictionary<string, FlowNode> nodeMap,
        FlowContext context,
        HashSet<string>? visitedMerge = null)
    {
        var currentKey = startKey;
        var visited = new HashSet<string>();
        const int maxSteps = 1000;
        var steps = 0;

        while (!string.IsNullOrEmpty(currentKey) && steps++ < maxSteps)
        {
            if (!nodeMap.TryGetValue(currentKey, out var currentNode))
                break;

            // MERGE 节点：等待所有分支汇聚，返回 merge 之后的节点
            if (currentNode.ElementType == "MERGE")
                return currentKey; // 把控制权返还给调用者（分支执行器）

            // END 节点：执行后结束
            if (currentNode.ElementType == "END")
            {
                await new EndNodeExecutor().ExecuteAsync(currentNode, context);
                return null;
            }

            // 检测死循环（CONDITION 节点不计入 visited）
            if (currentNode.ElementType != "CONDITION" && !visited.Add(currentKey))
                throw new InvalidOperationException($"检测到死循环，节点: {currentKey}");

            INodeExecutor executor = currentNode.ElementType switch
            {
                "START"     => new StartNodeExecutor(),
                "METHOD"    => new MethodNodeExecutor(_httpClientFactory),
                "ASSIGN"    => new AssignNodeExecutor(),
                "CODE"      => new CodeNodeExecutor(),
                "MYSQL" or "DB" => new MysqlNodeExecutor(_dataSources),
                "CONDITION" => new ConditionNodeExecutor(),
                _ => throw new InvalidOperationException($"未知节点类型: {currentNode.ElementType}")
            };

            if (currentNode.ElementType == "CONDITION")
            {
                // CONDITION：选出要走的分支 key
                var branchKey = await executor.ExecuteAsync(currentNode, context);
                if (string.IsNullOrEmpty(branchKey)) break;

                // 查找是否有 MERGE 节点
                var mergeKey = FindMergeNode(currentNode, nodeMap);

                if (mergeKey != null && nodeMap.ContainsKey(mergeKey))
                {
                    // 执行选中分支，直到遇到 MERGE 节点
                    await ExecuteFromNode(branchKey, nodeMap, context);
                    // 所有分支汇聚后，继续从 MERGE 之后的节点执行
                    var mergeNode = nodeMap[mergeKey];
                    currentKey = mergeNode.Outgoings.FirstOrDefault() ?? "";
                }
                else
                {
                    // 没有 MERGE 聚合，直接沿分支执行到底
                    currentKey = branchKey;
                }
                continue;
            }

            var nextKey = await executor.ExecuteAsync(currentNode, context);
            if (nextKey == null) break;
            currentKey = nextKey;
        }

        return null;
    }

    /// <summary>在 CONDITION 节点的所有分支中，找到共同的 MERGE 节点</summary>
    private static string? FindMergeNode(FlowNode conditionNode, Dictionary<string, FlowNode> nodeMap)
    {
        if (conditionNode.Conditions == null || conditionNode.Conditions.Count == 0)
            return null;

        // 检查每个分支的直接或间接后继是否有 MERGE 节点
        foreach (var cond in conditionNode.Conditions)
        {
            if (string.IsNullOrEmpty(cond.Outgoing)) continue;
            var mergeKey = TraverseFindMerge(cond.Outgoing, nodeMap, new HashSet<string>());
            if (mergeKey != null) return mergeKey;
        }
        return null;
    }

    private static string? TraverseFindMerge(string nodeKey, Dictionary<string, FlowNode> nodeMap, HashSet<string> visited)
    {
        if (string.IsNullOrEmpty(nodeKey) || !visited.Add(nodeKey)) return null;
        if (!nodeMap.TryGetValue(nodeKey, out var node)) return null;
        if (node.ElementType == "MERGE") return nodeKey;
        foreach (var next in node.Outgoings)
        {
            var found = TraverseFindMerge(next, nodeMap, visited);
            if (found != null) return found;
        }
        if (node.Conditions != null)
        {
            foreach (var cond in node.Conditions)
            {
                if (!string.IsNullOrEmpty(cond.Outgoing))
                {
                    var found = TraverseFindMerge(cond.Outgoing, nodeMap, visited);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }
}
