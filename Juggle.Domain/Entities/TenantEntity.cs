namespace Juggle.Domain.Entities;

/// <summary>
/// 租户实体
/// 用于多租户系统的租户管理，支持租户隔离和权限控制
/// </summary>
public class TenantEntity : BaseEntity
{
    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; } = "";

    /// <summary>
    /// 租户编码（唯一标识）
    /// </summary>
    public string? TenantCode { get; set; }

    /// <summary>
    /// 租户描述
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 租户状态：1-启用，0-禁用
    /// 禁用的租户下的用户无法登录
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 租户过期时间
    /// - null：永不过期
    /// - 有值：过期后自动禁用
    /// </summary>
    public DateTime? ExpiredAt { get; set; }

    /// <summary>
    /// 租户拥有的菜单权限（JSON 数组格式）
    /// 用于控制租户用户可访问的菜单模块
    /// 例如：["flow/define", "flow/log"]
    /// </summary>
    public string MenuKeys { get; set; } = "[]";
}
