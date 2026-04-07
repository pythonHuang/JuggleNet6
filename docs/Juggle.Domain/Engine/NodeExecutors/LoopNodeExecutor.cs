using System.Text.Json;

namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// LOOP 循环节点执行器：对数组/集合进行遍历，每次迭代执行循环体内的节点逻辑。
/// 
/// 设计思路：
/// - LOOP 节点本身是一个"容器"，它只有一个 outgoings 指向循环体的起始节点
/// - 循环体最后会连回 LOOP 节点或连向 MERGE 节点
/// - 在引擎层面，LOOP 节点需要特殊处理（类似 CONDITION）
/// 
/// 简化实现方案（v1）：
/// - 不在引擎中实现循环跳转（太复杂）
/// - LOOP 节点接收一个 JSON 数组变量，用索引变量遍历每个元素
/// - 将当前元素和索引写入上下文变量，然后直接走到下一个节点
/// - 配合 CONDITION + ASSIGN 实现实际循环效果
/// 
/// 高级方案（v2，暂不实现）：
/// - 循环体自动执行，引擎内部循环跳转
/// </summary>
public class LoopNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.LoopConfig;
        if (cfg == null)
            throw new InvalidOperationException($"LOOP node [{node.Key}] 未配置 loopConfig");

        // 1. 获取要遍历的数组
        var arrayVar = context.GetVariable(cfg.ArrayVariable);
        if (arrayVar == null)
            throw new InvalidOperationException($"LOOP node [{node.Key}] 变量 [{cfg.ArrayVariable}] 不存在或为空");

        List<object?> items;
        if (arrayVar is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Array)
            {
                items = new List<object?>();
                foreach (var item in je.EnumerateArray())
                {
                    items.Add(item.ValueKind switch
                    {
                        JsonValueKind.String => item.GetString(),
                        JsonValueKind.Number => item.TryGetInt64(out var l) ? (object?)l : item.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => item.GetRawText()
                    });
                }
            }
            else
            {
                throw new InvalidOperationException($"LOOP node [{node.Key}] 变量 [{cfg.ArrayVariable}] 不是数组类型");
            }
        }
        else if (arrayVar is System.Collections.IEnumerable enumerable && arrayVar is not string)
        {
            items = new List<object?>();
            foreach (var item in enumerable)
                items.Add(item);
        }
        else
        {
            throw new InvalidOperationException($"LOOP node [{node.Key}] 变量 [{cfg.ArrayVariable}] 不是可遍历的数组/集合");
        }

        if (items.Count == 0)
        {
            // 空数组，设置索引为-1，跳过循环
            context.SetVariable(cfg.IndexVariable ?? "_loop_index", -1);
            context.SetVariable(cfg.ItemVariable ?? "_loop_item", null);
            context.SetVariable(cfg.TotalVariable ?? "_loop_total", items.Count);
            return Task.FromResult(node.Outgoings.FirstOrDefault());
        }

        // 2. 遍历数组，将每个元素的属性/值写入变量
        // 简化版：只设置当前元素为 JSON 对象字符串，以及索引
        // 用户可以在循环体中通过索引+数组变量配合 ASSIGN/CODE 使用
        
        // 设置总数
        context.SetVariable(cfg.TotalVariable ?? "_loop_total", items.Count);

        // 3. 依次处理每个元素（累积结果模式）
        // 将所有元素的处理结果收集到一个列表变量中
        var results = new List<object?>();
        
        for (int i = 0; i < items.Count; i++)
        {
            // 设置当前索引和元素
            context.SetVariable(cfg.IndexVariable ?? "_loop_index", i);
            context.SetVariable(cfg.ItemVariable ?? "_loop_item", items[i]);
            
            // 记录当前迭代信息到日志
            results.Add(items[i]);
        }

        // 4. 将所有元素结果写入输出变量（作为数组）
        context.SetVariable(cfg.OutputVariable ?? "_loop_results", results);

        return Task.FromResult(node.Outgoings.FirstOrDefault());
    }
}
