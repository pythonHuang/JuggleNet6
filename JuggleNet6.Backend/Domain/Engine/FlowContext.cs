namespace JuggleNet6.Backend.Domain.Engine;

/// <summary>流程运行时上下文，持有变量容器 + 日志收集 + 静态变量引用</summary>
public class FlowContext
{
    public string FlowKey { get; set; } = "";
    public string Version { get; set; } = "";

    /// <summary>流程运行时变量（input_ / output_ / 中间变量）</summary>
    public Dictionary<string, object?> Variables { get; set; } = new();

    /// <summary>静态全局变量（VarCode → Value，流程内可读写，执行结束后写回 DB）</summary>
    public Dictionary<string, string?> StaticVariables { get; set; } = new();

    /// <summary>静态变量是否被修改（需要写回 DB）</summary>
    public HashSet<string> ModifiedStaticVarCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>节点执行日志列表（按执行顺序收集）</summary>
    public List<NodeLogEntry> NodeLogs { get; set; } = new();

    // ========== 流程变量操作 ==========

    public void SetVariable(string key, object? value)
    {
        Variables[key] = value;
    }

    public object? GetVariable(string key)
    {
        return Variables.TryGetValue(key, out var val) ? val : null;
    }

    // ========== 静态变量操作 ==========

    /// <summary>读取静态变量（返回字符串原始值）</summary>
    public string? GetStaticVariable(string code)
    {
        return StaticVariables.TryGetValue(code, out var val) ? val : null;
    }

    /// <summary>写入静态变量（标记为已修改，执行结束后持久化）</summary>
    public void SetStaticVariable(string code, string? value)
    {
        StaticVariables[code] = value;
        ModifiedStaticVarCodes.Add(code);
    }

    // ========== 节点日志操作 ==========

    public NodeLogEntry BeginNodeLog(string nodeKey, string nodeLabel, string nodeType)
    {
        var entry = new NodeLogEntry
        {
            SeqNo = NodeLogs.Count + 1,
            NodeKey = nodeKey,
            NodeLabel = nodeLabel,
            NodeType = nodeType,
            StartTime = DateTime.Now,
            Status = "RUNNING"
        };
        NodeLogs.Add(entry);
        return entry;
    }
}

/// <summary>节点执行日志条目（内存中收集，最后批量写 DB）</summary>
public class NodeLogEntry
{
    public int SeqNo { get; set; }
    public string NodeKey { get; set; } = "";
    public string NodeLabel { get; set; } = "";
    public string NodeType { get; set; } = "";
    public string Status { get; set; } = "RUNNING";  // RUNNING / SUCCESS / FAILED
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long CostMs => EndTime.HasValue ? (long)(EndTime.Value - StartTime).TotalMilliseconds : 0;
    public string? InputSnapshot { get; set; }
    public string? OutputSnapshot { get; set; }
    public string? Detail { get; set; }
    public string? ErrorMessage { get; set; }

    public void Complete(string status = "SUCCESS", string? errorMsg = null, string? detail = null)
    {
        EndTime = DateTime.Now;
        Status = status;
        ErrorMessage = errorMsg;
        if (detail != null) Detail = detail;
    }
}

/// <summary>流程执行结果</summary>
public class FlowResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object?> OutputData { get; set; } = new();
    public string? InstanceId { get; set; }   // 异步流程实例ID
    public long? LogId { get; set; }           // 日志记录ID（方便前端查日志）
    public long CostMs { get; set; }           // 总耗时（毫秒）
    /// <summary>执行完的上下文，含节点日志 + 静态变量修改记录（供持久化使用，不序列化到响应）</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public FlowContext? Context { get; set; }
}
