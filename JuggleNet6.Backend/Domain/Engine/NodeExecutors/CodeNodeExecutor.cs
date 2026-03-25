using System.Text.RegularExpressions;

namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

/// <summary>
/// CODE 代码节点执行器：执行用户自定义脚本（JavaScript 简化实现）
/// 支持 $var.getVariableValue('key') 和 $var.setVariableValue('key', value) 语法
/// </summary>
public class CodeNodeExecutor : INodeExecutor
{
    public Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var code = node.CodeConfig?.Script;
        if (string.IsNullOrWhiteSpace(code))
            return Task.FromResult(node.Outgoings.FirstOrDefault());

        try
        {
            ExecuteSimpleScript(code, context);
        }
        catch (Exception ex)
        {
            // 代码执行失败，记录错误但不中断流程（可根据需要改为抛出异常）
            context.SetVariable("_code_error_" + node.Key, ex.Message);
        }

        return Task.FromResult(node.Outgoings.FirstOrDefault());
    }

    /// <summary>
    /// 简化脚本解释器：解析 $var.setVariableValue('key', value) 语句
    /// 支持的语法：
    ///   $var.setVariableValue('varName', value)
    ///   $var.setVariableValue('varName', $var.getVariableValue('otherVar'))
    ///   // 注释行（忽略）
    ///   var x = $var.getVariableValue('varName')  （仅解析赋值目标）
    /// </summary>
    private static void ExecuteSimpleScript(string code, FlowContext context)
    {
        // 按行处理
        var lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var localVars = new Dictionary<string, object?>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                continue;

            // 匹配 $var.setVariableValue('key', value_expr)
            var setMatch = Regex.Match(line, @"\$var\.setVariableValue\s*\(\s*'([^']+)'\s*,\s*(.+?)\s*\)\s*;?$");
            if (setMatch.Success)
            {
                var targetKey = setMatch.Groups[1].Value;
                var valueExpr = setMatch.Groups[2].Value.Trim();
                var value = EvaluateExpression(valueExpr, context, localVars);
                context.SetVariable(targetKey, value);
                continue;
            }

            // 匹配局部变量声明: var/let/const x = expr
            var varDeclMatch = Regex.Match(line, @"^(?:var|let|const)\s+(\w+)\s*=\s*(.+?)\s*;?$");
            if (varDeclMatch.Success)
            {
                var localKey = varDeclMatch.Groups[1].Value;
                var valueExpr = varDeclMatch.Groups[2].Value.Trim();
                localVars[localKey] = EvaluateExpression(valueExpr, context, localVars);
                continue;
            }

            // 匹配 x = expr（不带 var/let/const）
            var assignMatch = Regex.Match(line, @"^(\w+)\s*=\s*(.+?)\s*;?$");
            if (assignMatch.Success && !line.Contains("=="))
            {
                var localKey = assignMatch.Groups[1].Value;
                var valueExpr = assignMatch.Groups[2].Value.Trim();
                localVars[localKey] = EvaluateExpression(valueExpr, context, localVars);
            }
        }
    }

    private static object? EvaluateExpression(string expr, FlowContext context, Dictionary<string, object?> localVars)
    {
        expr = expr.Trim();

        // $var.getVariableValue('key')
        var getMatch = Regex.Match(expr, @"\$var\.getVariableValue\s*\(\s*'([^']+)'\s*\)");
        if (getMatch.Success)
            return context.GetVariable(getMatch.Groups[1].Value);

        // 局部变量引用
        if (localVars.TryGetValue(expr, out var localVal))
            return localVal;

        // 字符串字面量 'value' 或 "value"
        if ((expr.StartsWith("'") && expr.EndsWith("'")) ||
            (expr.StartsWith("\"") && expr.EndsWith("\"")))
            return expr[1..^1];

        // 数字
        if (long.TryParse(expr, out var l)) return l;
        if (double.TryParse(expr, out var d)) return d;

        // 布尔
        if (expr == "true") return true;
        if (expr == "false") return false;
        if (expr == "null" || expr == "undefined") return null;

        // 字符串拼接（简单处理 "a" + b）
        if (expr.Contains('+'))
        {
            var parts = expr.Split('+');
            var sb = new System.Text.StringBuilder();
            foreach (var part in parts)
                sb.Append(EvaluateExpression(part.Trim(), context, localVars)?.ToString() ?? "");
            return sb.ToString();
        }

        return expr;
    }
}
