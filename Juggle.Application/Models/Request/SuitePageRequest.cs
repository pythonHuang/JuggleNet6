namespace Juggle.Application.Models.Request;

/// <summary>
/// 测试套件分页查询请求
/// </summary>
public class SuitePageRequest : PageRequest
{
    /// <summary>
    /// 测试套件名称（可选）
    /// </summary>
    public string? SuiteName { get; set; }
}