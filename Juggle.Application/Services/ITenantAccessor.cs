using System.Security.Claims;

namespace Juggle.Application.Services;

/// <summary>
/// 租户上下文访问接口
/// 用于在请求周期内获取当前用户的租户信息
/// 配合 ASP.NET Core 的 Scoped 生命周期，在中间件中设置
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    /// 当前请求的租户 ID
    /// null 表示未设置（超管或匿名请求）
    /// </summary>
    long? TenantId { get; set; }

    /// <summary>
    /// 当前请求的用户 ID
    /// </summary>
    long UserId { get; set; }

    /// <summary>
    /// 当前请求的用户角色 ID
    /// </summary>
    long? RoleId { get; set; }

    /// <summary>
    /// 当前用户是否为超级管理员（RoleId=1）
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// 当前请求的用户名
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// 从 ClaimsPrincipal 中加载用户信息
    /// 在登录认证中间件中调用
    /// </summary>
    /// <param name="user">当前 HTTP 请求的用户身份</param>
    void LoadFromClaims(ClaimsPrincipal user);
}
