namespace Juggle.Application.Services;

/// <summary>租户上下文访问器（Scoped，每个请求独立实例）。</summary>
public class TenantAccessor : ITenantAccessor
{
    public long? TenantId { get; set; }
}
