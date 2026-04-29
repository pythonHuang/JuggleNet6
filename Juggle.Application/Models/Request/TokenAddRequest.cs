namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增 Token 请求模型
/// </summary>
public class TokenAddRequest
{
    /// <summary>
    /// Token 名称（中文描述）
    /// </summary>
    public string TokenName { get; set; } = "";

    /// <summary>
    /// 过期时间（ISO 8601 格式，为空表示永不过期）
    /// </summary>
    public string? ExpiredAt { get; set; }
}