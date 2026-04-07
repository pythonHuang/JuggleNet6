namespace Juggle.Domain.Entities;

/// <summary>Token 权限配置：控制 Token 可访问的流程和套件接口</summary>
public class TokenPermissionEntity : BaseEntity
{
    /// <summary>关联的 Token ID</summary>
    public long TokenId { get; set; }

    /// <summary>权限类型：FLOW（流程） / API（套件接口）</summary>
    public string? PermissionType { get; set; }

    /// <summary>关联的资源 Key（flowKey 或 methodCode）</summary>
    public string? ResourceKey { get; set; }

    /// <summary>资源名称（冗余存储，方便展示）</summary>
    public string? ResourceName { get; set; }
}
