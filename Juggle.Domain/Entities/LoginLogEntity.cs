namespace Juggle.Domain.Entities;

/// <summary>登录访问日志</summary>
public class LoginLogEntity : BaseEntity
{
    /// <summary>用户ID</summary>
    public long? UserId { get; set; }
    /// <summary>用户名</summary>
    public string? UserName { get; set; }
    /// <summary>登录类型：login / logout</summary>
    public string LoginType { get; set; } = "login";
    /// <summary>登录结果：success / fail</summary>
    public string Result { get; set; } = "success";
    /// <summary>登录IP</summary>
    public string? IpAddress { get; set; }
    /// <summary>User-Agent</summary>
    public string? UserAgent { get; set; }
    // TenantId 继承自 BaseEntity
}
