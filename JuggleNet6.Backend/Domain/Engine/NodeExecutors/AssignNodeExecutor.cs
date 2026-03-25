namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

/// <summary>
/// ASSIGN 赋值节点执行器：根据赋值规则将常量或变量值写入目标变量
/// </summary>
public class AssignNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        if (node.AssignRules != null)
        {
            foreach (var rule in node.AssignRules)
            {
                object? value;
                if (rule.SourceType == "CONSTANT")
                {
                    // 常量赋值：直接使用 source 字符串值，尝试类型转换
                    value = ParseConstant(rule.Source, rule.DataType);
                }
                else
                {
                    // 变量赋值：从上下文取值
                    value = context.GetVariable(rule.Source);
                }
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
