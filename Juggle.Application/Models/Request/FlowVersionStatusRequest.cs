namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程版本状态更新请求
/// </summary>
public class FlowVersionStatusRequest
{
    /// <summary>
    /// 流程版本 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 状态值
    /// 0-草稿；1-已发布
    /// </summary>
    public int Status { get; set; }
}