namespace Juggle.Domain.Entities;

/// <summary>
/// 用户实体
/// 存储用户基本信息，包括登录凭证和角色关联
/// </summary>
public class UserEntity : BaseEntity
{
    /// <summary>
    /// 用户名（登录账号），唯一
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 密码（MD5 加密存储）
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 关联的角色 ID
    /// </summary>
    public long? RoleId { get; set; }

    // TenantId 继承自 BaseEntity，用于多租户隔离
}
