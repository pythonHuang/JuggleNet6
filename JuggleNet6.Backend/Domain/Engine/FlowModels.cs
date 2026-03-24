using System.Text.Json.Serialization;

namespace JuggleNet6.Backend.Domain.Engine;

/// <summary>流程节点定义</summary>
public class FlowNode
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("elementType")]
    public string ElementType { get; set; } = "";  // START END METHOD CONDITION

    [JsonPropertyName("incomings")]
    public List<string> Incomings { get; set; } = new();

    [JsonPropertyName("outgoings")]
    public List<string> Outgoings { get; set; } = new();

    [JsonPropertyName("method")]
    public MethodConfig? Method { get; set; }

    [JsonPropertyName("conditions")]
    public List<ConditionConfig>? Conditions { get; set; }
}

/// <summary>方法节点配置</summary>
public class MethodConfig
{
    [JsonPropertyName("suiteCode")]
    public string SuiteCode { get; set; } = "";

    [JsonPropertyName("methodCode")]
    public string MethodCode { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "GET";

    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = "JSON";

    [JsonPropertyName("inputFillRules")]
    public List<FillRule> InputFillRules { get; set; } = new();

    [JsonPropertyName("outputFillRules")]
    public List<FillRule> OutputFillRules { get; set; } = new();

    [JsonPropertyName("headerFillRules")]
    public List<FillRule> HeaderFillRules { get; set; } = new();
}

/// <summary>条件节点分支配置</summary>
public class ConditionConfig
{
    [JsonPropertyName("conditionName")]
    public string ConditionName { get; set; } = "";

    /// <summary>CUSTOM / DEFAULT</summary>
    [JsonPropertyName("conditionType")]
    public string ConditionType { get; set; } = "CUSTOM";

    /// <summary>条件表达式，如: env_isLogin == true</summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("outgoing")]
    public string? Outgoing { get; set; }
}

/// <summary>数据填充规则</summary>
public class FillRule
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    /// <summary>VARIABLE / CONSTANT</summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = "VARIABLE";

    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    /// <summary>INPUT_PARAM / OUTPUT_PARAM / HEADER / VARIABLE</summary>
    [JsonPropertyName("targetType")]
    public string TargetType { get; set; } = "INPUT_PARAM";
}
