namespace Juggle.Application.Models.Request;

/// <summary>
/// 数据源更新请求
/// </summary>
public class DataSourceUpdateRequest : DataSourceAddRequest
{
    /// <summary>
    /// 数据源 ID
    /// </summary>
    public long Id { get; set; }
}