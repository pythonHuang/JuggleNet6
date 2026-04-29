namespace Juggle.Application.Models.Request;

/// <summary>
/// 业务对象更新请求
/// 继承自 ObjectAddRequest
/// </summary>
public class ObjectUpdateRequest : ObjectAddRequest
{
    /// <summary>
    /// 业务对象 ID
    /// </summary>
    public long Id { get; set; }
}