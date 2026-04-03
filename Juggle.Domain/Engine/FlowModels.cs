using System.Text.Json.Serialization;

namespace Juggle.Domain.Engine;

/// <summary>流程节点定义</summary>
public class FlowNode
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("elementType")]
    public string ElementType { get; set; } = "";  // START END METHOD CONDITION ASSIGN CODE MYSQL(DB) MERGE

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("incomings")]
    public List<string> Incomings { get; set; } = new();

    [JsonPropertyName("outgoings")]
    public List<string> Outgoings { get; set; } = new();

    [JsonPropertyName("method")]
    public MethodConfig? Method { get; set; }

    [JsonPropertyName("conditions")]
    public List<ConditionConfig>? Conditions { get; set; }

    [JsonPropertyName("assignRules")]
    public List<AssignRule>? AssignRules { get; set; }

    [JsonPropertyName("codeConfig")]
    public CodeConfig? CodeConfig { get; set; }

    [JsonPropertyName("mysqlConfig")]
    public MysqlConfig? MysqlConfig { get; set; }

    [JsonPropertyName("subFlowConfig")]
    public SubFlowConfig? SubFlowConfig { get; set; }

    /// <summary>节点执行超时时间（毫秒），0或不设置表示不限制</summary>
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 0;

    /// <summary>失败重试次数，0或不设置表示不重试</summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; } = 0;

    /// <summary>重试间隔（毫秒），默认1000ms</summary>
    [JsonPropertyName("retryInterval")]
    public int RetryInterval { get; set; } = 1000;
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

    /// <summary>HTTP WEBSERVICE</summary>
    [JsonPropertyName("methodType")]
    public string MethodType { get; set; } = "HTTP";

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

    /// <summary>INPUT_PARAM / OUTPUT_PARAM / HEADER / VARIABLE / OUTPUT（流程输出参数）</summary>
    [JsonPropertyName("targetType")]
    public string TargetType { get; set; } = "INPUT_PARAM";
}

/// <summary>赋值规则（ASSIGN 节点）</summary>
public class AssignRule
{
    /// <summary>来源：变量名或常量值</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    /// <summary>VARIABLE / CONSTANT / STATIC（全局静态变量）</summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = "VARIABLE";

    /// <summary>目标变量名</summary>
    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    /// <summary>目标类型：VARIABLE（流程变量）/ STATIC（全局静态变量）</summary>
    [JsonPropertyName("targetType")]
    public string TargetType { get; set; } = "VARIABLE";

    /// <summary>数据类型（用于常量解析）：string/integer/double/boolean</summary>
    [JsonPropertyName("dataType")]
    public string? DataType { get; set; }
}

/// <summary>代码节点配置（CODE 节点）</summary>
public class CodeConfig
{
    /// <summary>脚本语言：javascript / groovy（当前实现支持简化 JS 语法）</summary>
    [JsonPropertyName("scriptType")]
    public string ScriptType { get; set; } = "javascript";

    /// <summary>脚本内容</summary>
    [JsonPropertyName("script")]
    public string Script { get; set; } = "";
}

/// <summary>子流程节点配置（SUB_FLOW 节点）</summary>
public class SubFlowConfig
{
    /// <summary>被调用的子流程 Key</summary>
    [JsonPropertyName("subFlowKey")]
    public string SubFlowKey { get; set; } = "";

    /// <summary>入参映射：从当前流程变量填充子流程入参</summary>
    [JsonPropertyName("inputMappings")]
    public List<SubFlowMapping> InputMappings { get; set; } = new();

    /// <summary>出参映射：将子流程输出变量写回当前流程变量</summary>
    [JsonPropertyName("outputMappings")]
    public List<SubFlowMapping> OutputMappings { get; set; } = new();
}

/// <summary>子流程变量映射规则</summary>
public class SubFlowMapping
{
    /// <summary>来源名称（当前流程变量名 或 子流程输出变量名）</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    /// <summary>VARIABLE / CONSTANT</summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = "VARIABLE";

    /// <summary>目标名称（子流程入参名 或 当前流程变量名）</summary>
    [JsonPropertyName("target")]
    public string Target { get; set; } = "";
}

/// <summary>MySQL 节点配置（MYSQL 节点）</summary>
public class MysqlConfig
{
    /// <summary>数据源名称（关联系统设置中的数据源）</summary>
    [JsonPropertyName("dataSourceName")]
    public string DataSourceName { get; set; } = "";

    /// <summary>数据源类型：mysql / sqlite</summary>
    [JsonPropertyName("dataSourceType")]
    public string? DataSourceType { get; set; }

    /// <summary>SQL 语句，支持 ${varName} 模板变量</summary>
    [JsonPropertyName("sql")]
    public string Sql { get; set; } = "";

    /// <summary>操作类型：QUERY（查询）/ UPDATE（更改）</summary>
    [JsonPropertyName("operationType")]
    public string OperationType { get; set; } = "QUERY";

    /// <summary>查询结果写入的变量名（QUERY 类型有效）</summary>
    [JsonPropertyName("outputVariable")]
    public string? OutputVariable { get; set; }

    /// <summary>影响行数写入的变量名（UPDATE/INSERT/DELETE 类型有效）</summary>
    [JsonPropertyName("affectedRowsVariable")]
    public string? AffectedRowsVariable { get; set; }

    /// <summary>查询结果目标类型：VARIABLE（变量）/ OUTPUT（输出参数）</summary>
    [JsonPropertyName("outputTargetType")]
    public string? OutputTargetType { get; set; }

    /// <summary>影响行数目标类型：VARIABLE（变量）/ OUTPUT（输出参数）</summary>
    [JsonPropertyName("affectedTargetType")]
    public string? AffectedTargetType { get; set; }
}
