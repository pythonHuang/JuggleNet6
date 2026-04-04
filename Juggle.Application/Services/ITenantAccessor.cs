namespace Juggle.Application.Services;

/// <summary>
/// 租户上下文访问接口，每个请求的租户ID通过此接口获取。
/// 配合 ASP.NET Core 的 Scoped 生命周期，在中间件中设置当前租户。
/// </summary>
public interface ITenantAccessor
{
    /// <summary>当前请求的租户ID，null 表示未设置（超级管理可能跨租户）。</summary>
    long? TenantId { get; set; }
}
