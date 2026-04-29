namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程测试用例分页查询请求
/// </summary>
public class FlowTestCasePageRequest : PageRequest
{
    /// <summary>
    /// 流程标识 Key（可选）
    /// 用于筛选指定流程的测试用例
    /// </summary>
    public string? FlowKey { get; set; }
}