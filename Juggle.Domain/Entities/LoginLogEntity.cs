namespace Juggle.Domain.Entities;

/// <summary>
/// 登录访问日志实体
/// 记录用户的登录行为，包括成功和失败的登录尝试
/// </summary>
public class LoginLogEntity : BaseEntity
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 登录类型：
    /// - login：登录
    /// - logout：登出
    /// </summary>
    public string LoginType { get; set; } = "login";

    /// <summary>
    /// 登录结果：
    /// - success：登录成功
    /// - fail：登录失败
    /// </summary>
    public string Result { get; set; } = "success";

    /// <summary>
    /// 登录来源 IP 地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 客户端 User-Agent 信息
    /// 包含浏览器、操作系统等客户端标识
    /// </summary>
    public string? UserAgent { get; set; }

    // TenantId 继承自 BaseEntity
}
