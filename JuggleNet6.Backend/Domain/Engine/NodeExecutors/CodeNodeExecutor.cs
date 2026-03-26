using System.Text.RegularExpressions;

namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

/// <summary>
/// CODE 代码节点执行器：执行用户自定义脚本（JavaScript 简化实现）
/// 支持语法：
///   $var.getVariableValue('key')                  — 读取流程变量
///   $var.setVariableValue('key', value)           — 写入流程变量
///   $static.getVariableValue('code')              — 读取全局静态变量
///   $static.setVariableValue('code', value)       — 写入全局静态变量（执行后持久化）
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
            context.SetVariable("_code_error_" + node.Key, ex.Message);
        }

        return Task.FromResult(node.Outgoings.FirstOrDefault());
    }

    private static void ExecuteSimpleScript(string code, FlowContext context)
    {
        var lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var localVars = new Dictionary<string, object?>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                continue;

            // $var.setVariableValue('key', expr)
            var setVarMatch = Regex.Match(line, @"\$var\.setVariableValue\s*\(\s*'([^']+)'\s*,\s*(.+?)\s*\)\s*;?$");
            if (setVarMatch.Success)
            {
                var targetKey = setVarMatch.Groups[1].Value;
                var valueExpr = setVarMatch.Groups[2].Value.Trim();
                context.SetVariable(targetKey, EvaluateExpression(valueExpr, context, localVars));
                continue;
            }

            // $static.setVariableValue('code', expr)
            var setStaticMatch = Regex.Match(line, @"\$static\.setVariableValue\s*\(\s*'([^']+)'\s*,\s*(.+?)\s*\)\s*;?$");
            if (setStaticMatch.Success)
            {
                var code2 = setStaticMatch.Groups[1].Value;
                var valueExpr = setStaticMatch.Groups[2].Value.Trim();
                var val = EvaluateExpression(valueExpr, context, localVars);
                context.SetStaticVariable(code2, val?.ToString());
                continue;
            }

            // 局部变量声明: var/let/const x = expr
            var varDeclMatch = Regex.Match(line, @"^(?:var|let|const)\s+(\w+)\s*=\s*(.+?)\s*;?$");
            if (varDeclMatch.Success)
            {
                var localKey = varDeclMatch.Groups[1].Value;
                var valueExpr = varDeclMatch.Groups[2].Value.Trim();
                localVars[localKey] = EvaluateExpression(valueExpr, context, localVars);
                continue;
            }

            // x = expr（不带关键字）
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
        var getVarMatch = Regex.Match(expr, @"\$var\.getVariableValue\s*\(\s*'([^']+)'\s*\)");
        if (getVarMatch.Success)
            return context.GetVariable(getVarMatch.Groups[1].Value);

        // $static.getVariableValue('code')
        var getStaticMatch = Regex.Match(expr, @"\$static\.getVariableValue\s*\(\s*'([^']+)'\s*\)");
        if (getStaticMatch.Success)
            return context.GetStaticVariable(getStaticMatch.Groups[1].Value);

        // 局部变量引用
        if (localVars.TryGetValue(expr, out var localVal))
            return localVal;

        // 字符串字面量
        if ((expr.StartsWith("'") && expr.EndsWith("'")) ||
            (expr.StartsWith("\"") && expr.EndsWith("\"")))
            return expr[1..^1];

        // 数字
        if (long.TryParse(expr, out var l)) return l;
        if (double.TryParse(expr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;

        // 布尔
        if (expr == "true") return true;
        if (expr == "false") return false;
        if (expr == "null" || expr == "undefined") return null;

        // 字符串拼接（a + b）
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
