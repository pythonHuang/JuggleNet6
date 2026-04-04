using System.Security.Claims;

namespace Juggle.Application.Services;

/// <summary>租户上下文访问器（Scoped，每个请求独立实例）。</summary>
public class TenantAccessor : ITenantAccessor
{
    public long? TenantId { get; set; }
    public long UserId { get; set; }
    public long? RoleId { get; set; }
    public bool IsSuperAdmin => RoleId == 1;
    public string UserName { get; set; } = "";

    public void LoadFromClaims(ClaimsPrincipal user)
    {
        var nameIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdClaim != null && long.TryParse(nameIdClaim.Value, out var uid))
            UserId = uid;

        UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? "";

        var roleClaim = user.FindFirst("RoleId");
        if (roleClaim != null && long.TryParse(roleClaim.Value, out var rid))
            RoleId = rid;

        var tenantClaim = user.FindFirst("TenantId");
        if (tenantClaim != null && long.TryParse(tenantClaim.Value, out var tid))
            TenantId = tid;
    }
}
