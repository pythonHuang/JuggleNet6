namespace Juggle.Application.Models.Request;

/// <summary>
/// 用户修改密码请求
/// 用户自行修改密码，需验证原密码
/// </summary>
public class UserChangePwdRequest
{
    /// <summary>
    /// 原密码
    /// </summary>
    public string OldPassword { get; set; } = "";

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = "";
}