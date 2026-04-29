namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增用户请求模型
/// </summary>
public class UserAddRequest
{
    /// <summary>
    /// 用户名（登录账号，唯一）
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 密码（明文，后端 MD5 加密存储）
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// 关联的角色 ID
    /// </summary>
    public long? RoleId { get; set; }

    /// <summary>
    /// 所属租户 ID
    /// </summary>
    public long? TenantId { get; set; }
}