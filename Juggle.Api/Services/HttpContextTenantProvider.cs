using Juggle.Infrastructure.Persistence;

namespace Juggle.Api.Services;

/// <summary>
/// 从当前 HTTP 请求的 JWT Claims 中获取租户 ID。
/// 超级管理员（RoleId=1）返回 null，DbContext 不加过滤。
/// </summary>
public class HttpContextTenantProvider : ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? GetCurrentTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return null;

        // 超级管理员（RoleId=1）不受租户过滤，可看所有数据
        var roleClaim = user.FindFirst("RoleId")?.Value;
        if (roleClaim == "1") return null;

        var tenantClaim = user.FindFirst("TenantId")?.Value;
        return long.TryParse(tenantClaim, out var tid) ? tid : null;
    }
}
