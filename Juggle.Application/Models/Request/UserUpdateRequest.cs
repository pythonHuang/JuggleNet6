namespace Juggle.Application.Models.Request;

/// <summary>
/// 用户更新请求
/// </summary>
public class UserUpdateRequest
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名称
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 角色 ID（可选）
    /// </summary>
    public long? RoleId { get; set; }

    /// <summary>
    /// 租户 ID（可选）
    /// </summary>
    public long? TenantId { get; set; }
}