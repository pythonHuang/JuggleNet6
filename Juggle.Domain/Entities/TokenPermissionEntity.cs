namespace Juggle.Domain.Entities;

/// <summary>
/// Token 权限配置实体
/// 用于控制 API Token 可访问的资源范围
/// 关联 TokenEntity，建立细粒度的权限控制
/// </summary>
public class TokenPermissionEntity : BaseEntity
{
    /// <summary>
    /// 关联的 Token ID
    /// </summary>
    public long TokenId { get; set; }

    /// <summary>
    /// 权限类型：
    /// - FLOW：流程资源
    /// - API：套件接口资源
    /// </summary>
    public string? PermissionType { get; set; }

    /// <summary>
    /// 关联的资源 Key
    /// - FLOW 类型：flowKey
    /// - API 类型：methodCode
    /// </summary>
    public string? ResourceKey { get; set; }

    /// <summary>
    /// 资源名称（冗余存储，便于展示）
    /// </summary>
    public string? ResourceName { get; set; }
}
