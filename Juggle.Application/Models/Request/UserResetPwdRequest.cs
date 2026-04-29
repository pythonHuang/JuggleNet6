namespace Juggle.Application.Models.Request;

/// <summary>
/// 重置用户密码请求
/// 管理员可为用户重置密码
/// </summary>
public class UserResetPwdRequest
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = "";
}