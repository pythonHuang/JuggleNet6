namespace Juggle.Domain.Entities;

/// <summary>
/// API Token 实体
/// 用于开放接口的身份认证，支持设置权限范围和过期时间
/// </summary>
public class TokenEntity : BaseEntity
{
    /// <summary>
    /// Token 值（唯一标识）
    /// 调用开放接口时通过此 Token 进行身份验证
    /// </summary>
    public string? TokenValue { get; set; }

    /// <summary>
    /// Token 名称（中文描述）
    /// </summary>
    public string? TokenName { get; set; }

    /// <summary>
    /// 过期时间（ISO 8601 格式字符串）
    /// 为空表示永不过期
    /// </summary>
    public string? ExpiredAt { get; set; }

    /// <summary>
    /// 状态：1-启用，0-禁用
    /// </summary>
    public int Status { get; set; } = 1;
}
