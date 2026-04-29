namespace Juggle.Application.Models.Request;

/// <summary>
/// 用户登录请求模型
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名（登录账号）
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 密码（明文，后端 MD5 加密后比对）
    /// </summary>
    public string Password { get; set; } = "";
}