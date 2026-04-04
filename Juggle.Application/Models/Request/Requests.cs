namespace Juggle.Application.Models.Request;

public class PageRequest
{
    public int PageNum { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
}

// User Management
public class UserAddRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public long? RoleId { get; set; }
    public long? TenantId { get; set; }
}

public class UserUpdateRequest
{
    public long Id { get; set; }
    public string UserName { get; set; } = "";
    public long? RoleId { get; set; }
    public long? TenantId { get; set; }
}

public class UserResetPwdRequest
{
    public long Id { get; set; }
    public string NewPassword { get; set; } = "";
}

public class UserChangePwdRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

// SystemConfig
public class SystemConfigSaveRequest
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
}

// FlowTestCase
public class FlowTestCaseSaveRequest
{
    public long? Id { get; set; }
    public string FlowKey { get; set; } = "";
    public string CaseName { get; set; } = "";
    public string? InputJson { get; set; }
    public string? AssertJson { get; set; }
    public string? Remark { get; set; }
}

public class FlowTestCasePageRequest : PageRequest
{
    public string? FlowKey { get; set; }
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
    /// <summary>HTTP WEBSERVICE</summary>
    public string MethodType { get; set; } = "HTTP";
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
    public string? GroupName { get; set; }
}

public class FlowDefinitionUpdateRequest
{
    public long Id { get; set; }
    public string FlowName { get; set; } = "";
    public string? FlowDesc { get; set; }
    public string FlowType { get; set; } = "sync";
    public string? GroupName { get; set; }
}

public class FlowDefinitionSaveRequest
{
    public long Id { get; set; }
    public string FlowContent { get; set; } = "[]";
}

public class FlowDefinitionPageRequest : PageRequest
{
    public string? FlowName { get; set; }
    public string? GroupName { get; set; }
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

// Token 权限
public class TokenPermissionSaveRequest
{
    public string PermissionType { get; set; } = "";  // FLOW / API
    public string ResourceKey { get; set; } = "";     // flowKey / methodCode
    public string? ResourceName { get; set; }
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

// FlowLog
public class FlowLogPageRequest : PageRequest
{
    public string? FlowKey   { get; set; }
    public string? Status    { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate   { get; set; }
}

// StaticVariable
public class StaticVarSaveRequest
{
    public long?   Id           { get; set; }
    public string? VarCode      { get; set; }
    public string? VarName      { get; set; }
    public string? DataType     { get; set; }
    public string? Value        { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description  { get; set; }
    public string? GroupName    { get; set; }
}

public class SetValueRequest
{
    public string? Value { get; set; }
}

// ScheduleTask
public class ScheduleTaskAddRequest
{
    public string FlowKey { get; set; } = "";
    public string? FlowName { get; set; }
    public string CronExpression { get; set; } = "0 */5 * * * *";
    public string? InputJson { get; set; }
}

public class ScheduleTaskUpdateRequest : ScheduleTaskAddRequest
{
    public long Id { get; set; }
    public int Status { get; set; }
}

public class ScheduleTaskPageRequest : PageRequest
{
    public string? FlowKey { get; set; }
    public int? Status { get; set; }
}

// Role
public class RoleAddRequest
{
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
    public long? TenantId { get; set; }
    public List<string> MenuKeys { get; set; } = new();
}

public class RoleUpdateRequest
{
    public long Id { get; set; }
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
    public long? TenantId { get; set; }
    public List<string> MenuKeys { get; set; } = new();
}

// Tenant
public class TenantAddRequest
{
    public string TenantName { get; set; } = "";
    public string? TenantCode { get; set; }
    public string? Remark { get; set; }
    /// <summary>过期时间（null 表示永不过期）</summary>
    public DateTime? ExpiredAt { get; set; }
    /// <summary>租户菜单权限列表</summary>
    public List<string> MenuKeys { get; set; } = new();
    /// <summary>关联用户ID列表</summary>
    public List<long> UserIds { get; set; } = new();
}

public class TenantUpdateRequest
{
    public long Id { get; set; }
    public string TenantName { get; set; } = "";
    public string? TenantCode { get; set; }
    public string? Remark { get; set; }
    public int Status { get; set; }
    /// <summary>过期时间（null 表示永不过期）</summary>
    public DateTime? ExpiredAt { get; set; }
    /// <summary>租户菜单权限列表</summary>
    public List<string> MenuKeys { get; set; } = new();
    /// <summary>关联用户ID列表</summary>
    public List<long> UserIds { get; set; } = new();
}

// AuditLog
public class AuditLogPageRequest : PageRequest
{
    public string? Module { get; set; }
    public string? ActionType { get; set; }
}

