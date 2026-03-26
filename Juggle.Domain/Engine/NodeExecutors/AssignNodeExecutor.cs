namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// ASSIGN 赋值节点执行器：根据赋值规则将常量或变量值写入目标变量
/// sourceType 支持：
///   CONSTANT      — 常量值
///   VARIABLE      — 流程变量
///   STATIC        — 全局静态变量（$static.xxx）
/// targetType 支持：
///   (默认)         — 流程变量
///   STATIC        — 写入全局静态变量（执行后持久化）
/// </summary>
public class AssignNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        if (node.AssignRules != null)
        {
            foreach (var rule in node.AssignRules)
            {
                // ==== 读取来源值 ====
                object? value;
                var srcType = (rule.SourceType ?? "VARIABLE").ToUpper();
                switch (srcType)
                {
                    case "CONSTANT":
                        value = ParseConstant(rule.Source, rule.DataType);
                        break;
                    case "STATIC":
                        // 从全局静态变量读取
                        value = context.GetStaticVariable(rule.Source);
                        break;
                    default:
                        // VARIABLE
                        value = context.GetVariable(rule.Source);
                        break;
                }

                // ==== 写入目标 ====
                var tgtType = (rule.TargetType ?? "VARIABLE").ToUpper();
                if (tgtType == "STATIC")
                    context.SetStaticVariable(rule.Target, value?.ToString());
                else
                    context.SetVariable(rule.Target, value);
            }
        }

        return Task.FromResult(node.Outgoings.FirstOrDefault());
    }

    private static object? ParseConstant(string source, string? dataType)
    {
        if (string.IsNullOrEmpty(source)) return null;
        return (dataType?.ToLower()) switch
        {
            "integer" or "int" => int.TryParse(source, out var i) ? i : (object?)source,
            "double" or "float" or "decimal" => double.TryParse(source, out var d) ? d : (object?)source,
            "boolean" or "bool" => bool.TryParse(source, out var b) ? b : (object?)source,
            _ => source
        };
    }
}
