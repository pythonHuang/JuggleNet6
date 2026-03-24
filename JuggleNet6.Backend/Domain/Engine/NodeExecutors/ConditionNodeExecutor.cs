namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

/// <summary>CONDITION 节点执行器：按条件表达式选择下一个分支</summary>
public class ConditionNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        if (node.Conditions == null || node.Conditions.Count == 0)
            return Task.FromResult<string?>(null);

        string? defaultOutgoing = null;

        foreach (var condition in node.Conditions)
        {
            if (condition.ConditionType == "DEFAULT")
            {
                defaultOutgoing = condition.Outgoing;
                continue;
            }

            // 计算表达式
            if (!string.IsNullOrWhiteSpace(condition.Expression))
            {
                var result = EvaluateExpression(condition.Expression, context);
                if (result)
                    return Task.FromResult(condition.Outgoing);
            }
        }

        // 没有匹配，走 DEFAULT
        return Task.FromResult(defaultOutgoing);
    }

    /// <summary>
    /// 简单条件表达式求值，支持：
    ///   变量 == 值
    ///   变量 != 值
    ///   变量 > 数值
    ///   变量 < 数值
    ///   变量 >= 数值
    ///   变量 <= 数值
    /// </summary>
    private static bool EvaluateExpression(string expression, FlowContext context)
    {
        try
        {
            // 支持的操作符（顺序注意，先长后短）
            string[] ops = { ">=", "<=", "!=", "==", ">", "<" };
            foreach (var op in ops)
            {
                var idx = expression.IndexOf(op, StringComparison.Ordinal);
                if (idx < 0) continue;

                var left = expression[..idx].Trim();
                var right = expression[(idx + op.Length)..].Trim().Trim('"', '\'');

                var leftVal = context.GetVariable(left)?.ToString() ?? "";

                return op switch
                {
                    "==" => string.Equals(leftVal, right, StringComparison.OrdinalIgnoreCase),
                    "!=" => !string.Equals(leftVal, right, StringComparison.OrdinalIgnoreCase),
                    ">"  => double.TryParse(leftVal, out var l1) && double.TryParse(right, out var r1) && l1 > r1,
                    "<"  => double.TryParse(leftVal, out var l2) && double.TryParse(right, out var r2) && l2 < r2,
                    ">=" => double.TryParse(leftVal, out var l3) && double.TryParse(right, out var r3) && l3 >= r3,
                    "<=" => double.TryParse(leftVal, out var l4) && double.TryParse(right, out var r4) && l4 <= r4,
                    _ => false
                };
            }

            // 没有操作符，判断变量是否为 true
            var varVal = context.GetVariable(expression.Trim())?.ToString() ?? "";
            return varVal.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
