namespace Juggle.Domain.Entities;

/// <summary>流程测试用例</summary>
public class FlowTestCaseEntity : BaseEntity
{
    public string FlowKey { get; set; } = "";
    public string CaseName { get; set; } = "";
    public string? InputJson { get; set; }     // 入参 JSON
    public string? AssertJson { get; set; }    // 断言 JSON：{"varName": "expectedValue"}
    public string? LastRunStatus { get; set; } // SUCCESS/FAILED/PENDING
    public string? LastRunTime { get; set; }
    public string? LastRunResult { get; set; } // 最近执行结果摘要
    public string? Remark { get; set; }
}
