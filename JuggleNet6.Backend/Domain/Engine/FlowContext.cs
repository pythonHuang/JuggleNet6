namespace JuggleNet6.Backend.Domain.Engine;

/// <summary>流程运行时上下文，持有变量容器</summary>
public class FlowContext
{
    public string FlowKey { get; set; } = "";
    public string Version { get; set; } = "";
    public Dictionary<string, object?> Variables { get; set; } = new();

    public void SetVariable(string key, object? value)
    {
        Variables[key] = value;
    }

    public object? GetVariable(string key)
    {
        return Variables.TryGetValue(key, out var val) ? val : null;
    }
}

/// <summary>流程执行结果</summary>
public class FlowResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object?> OutputData { get; set; } = new();
    public string? InstanceId { get; set; }  // 异步流程实例ID
}
