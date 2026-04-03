namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// WAIT/DELAY 延迟节点执行器：流程暂停指定时间后继续执行。
/// 支持固定秒数和变量引用两种模式。
/// </summary>
public class DelayNodeExecutor : INodeExecutor
{
    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.DelayConfig;
        if (cfg == null)
            throw new InvalidOperationException($"DELAY node [{node.Key}] 未配置 delayConfig");

        // 获取延迟时间（毫秒）
        int delayMs;
        if (cfg.VariableMode && !string.IsNullOrEmpty(cfg.DelayVariable))
        {
            // 从变量读取延迟时间
            var val = context.GetVariable(cfg.DelayVariable);
            if (val == null)
                throw new InvalidOperationException($"DELAY node [{node.Key}] 变量 [{cfg.DelayVariable}] 不存在");
            
            if (!int.TryParse(val.ToString(), out delayMs) || delayMs <= 0)
                throw new InvalidOperationException($"DELAY node [{node.Key}] 变量 [{cfg.DelayVariable}] 的值不是有效的正整数毫秒");
        }
        else
        {
            delayMs = cfg.DelayMs > 0 ? cfg.DelayMs : 1000;
        }

        await Task.Delay(delayMs);

        return node.Outgoings.FirstOrDefault();
    }
}
