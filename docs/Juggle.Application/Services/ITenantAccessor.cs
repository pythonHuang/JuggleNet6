using System.Security.Claims;

namespace Juggle.Application.Services;

/// <summary>
/// 租户上下文访问接口，每个请求的租户信息通过此接口获取。
/// 配合 ASP.NET Core 的 Scoped 生命周期，在中间件中设置。
/// </summary>
public interface ITenantAccessor
{
    /// <summary>当前请求的租户ID，null 表示未设置。</summary>
    long? TenantId { get; set; }
    /// <summary>当前请求的用户ID。</summary>
    long UserId { get; set; }
    /// <summary>当前请求的用户角色ID。</summary>
    long? RoleId { get; set; }
    /// <summary>当前用户是否为超级管理员（RoleId=1）。</summary>
    bool IsSuperAdmin { get; }
    /// <summary>当前请求的用户名。</summary>
    string UserName { get; set; }

    /// <summary>从 ClaimsPrincipal 中加载用户信息。</summary>
    void LoadFromClaims(ClaimsPrincipal user);
}
