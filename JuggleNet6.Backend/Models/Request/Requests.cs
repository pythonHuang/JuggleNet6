namespace JuggleNet6.Backend.Models.Request;

public class PageRequest
{
    public int PageNum { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

// Suite
public class SuiteAddRequest
{
    public string SuiteName { get; set; } = "";
    public string? SuiteDesc { get; set; }
    public string? SuiteImage { get; set; }
    public string SuiteVersion { get; set; } = "v1.0.0";
}

public class SuiteUpdateRequest : SuiteAddRequest
{
    public long Id { get; set; }
}

public class SuitePageRequest : PageRequest
{
    public string? SuiteName { get; set; }
}

// API
public class ApiAddRequest
{
    public string SuiteCode { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string? MethodDesc { get; set; }
    public string Url { get; set; } = "";
    public string RequestType { get; set; } = "GET";
    public string ContentType { get; set; } = "JSON";
    public string? MockJson { get; set; }
}

public class ApiUpdateRequest : ApiAddRequest
{
    public long Id { get; set; }
}

public class ApiDebugRequest
{
    public long ApiId { get; set; }
    public Dictionary<string, object?> Headers { get; set; } = new();
    public Dictionary<string, object?> Params { get; set; } = new();
}

// Parameter
public class ParameterSaveRequest
{
    public long OwnerId { get; set; }
    public string OwnerCode { get; set; } = "";
    public int ParamType { get; set; }
    public List<ParameterItem> Parameters { get; set; } = new();
}

public class ParameterItem
{
    public long? Id { get; set; }
    public string ParamCode { get; set; } = "";
    public string ParamName { get; set; } = "";
    public string DataType { get; set; } = "string";
    public string? ObjectCode { get; set; }
    public int Required { get; set; } = 0;
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public int SortNum { get; set; }
}

// Object
public class ObjectAddRequest
{
    public string ObjectName { get; set; } = "";
    public string? ObjectDesc { get; set; }
}

public class ObjectUpdateRequest : ObjectAddRequest
{
    public long Id { get; set; }
}

// FlowDefinition
public class FlowDefinitionAddRequest
{
    public string FlowName { get; set; } = "";
    public string? FlowDesc { get; set; }
    public string FlowType { get; set; } = "sync";
}

public class FlowDefinitionUpdateRequest
{
    public long Id { get; set; }
    public string FlowName { get; set; } = "";
    public string? FlowDesc { get; set; }
    public string FlowType { get; set; } = "sync";
}

public class FlowDefinitionSaveRequest
{
    public long Id { get; set; }
    public string FlowContent { get; set; } = "[]";
}

public class FlowDefinitionPageRequest : PageRequest
{
    public string? FlowName { get; set; }
}

public class FlowDeployRequest
{
    public long FlowDefinitionId { get; set; }
}

public class FlowDebugRequest
{
    public Dictionary<string, object?> Params { get; set; } = new();
}

// FlowVersion
public class FlowVersionPageRequest : PageRequest
{
    public string? FlowKey { get; set; }
}

public class FlowVersionStatusRequest
{
    public long Id { get; set; }
    public int Status { get; set; }
}

// Token
public class TokenAddRequest
{
    public string TokenName { get; set; } = "";
    public string? ExpiredAt { get; set; }
}

// DataSource
public class DataSourceAddRequest
{
    public string DsName { get; set; } = "";
    public string DsType { get; set; } = "mysql";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 3306;
    public string DbName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class DataSourceUpdateRequest : DataSourceAddRequest
{
    public long Id { get; set; }
}
