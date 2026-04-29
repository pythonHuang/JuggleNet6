namespace Juggle.Application.Models.Request;

/// <summary>
/// API 更新请求
/// 继承自 ApiAddRequest，包含新增和更新所需的所有字段
/// </summary>
public class ApiUpdateRequest : ApiAddRequest
{
    /// <summary>
    /// API 记录 ID（必填）
    /// </summary>
    public long Id { get; set; }
}