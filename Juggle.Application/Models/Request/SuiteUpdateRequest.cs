namespace Juggle.Application.Models.Request;

/// <summary>
/// 测试套件更新请求
/// </summary>
public class SuiteUpdateRequest : SuiteAddRequest
{
    /// <summary>
    /// 测试套件 ID
    /// </summary>
    public long Id { get; set; }
}