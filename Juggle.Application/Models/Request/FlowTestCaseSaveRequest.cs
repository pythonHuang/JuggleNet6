namespace Juggle.Application.Models.Request;

/// <summary>
/// 保存流程测试用例请求模型
/// </summary>
public class FlowTestCaseSaveRequest
{
    /// <summary>
    /// 测试用例 ID（编辑时传入，新增时为空）
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 关联的流程 Key
    /// </summary>
    public string FlowKey { get; set; } = "";

    /// <summary>
    /// 用例名称
    /// </summary>
    public string CaseName { get; set; } = "";

    /// <summary>
    /// 输入参数 JSON
    /// </summary>
    public string? InputJson { get; set; }

    /// <summary>
    /// 断言配置 JSON
    /// </summary>
    public string? AssertJson { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    public string? Remark { get; set; }
}