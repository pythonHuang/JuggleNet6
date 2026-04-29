namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程部署请求
/// 将流程定义发布到正式环境
/// </summary>
public class FlowDeployRequest
{
    /// <summary>
    /// 流程定义 ID
    /// </summary>
    public long FlowDefinitionId { get; set; }
}