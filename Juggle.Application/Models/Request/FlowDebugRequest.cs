namespace Juggle.Application.Models.Request;

/// <summary>
/// 流程调试请求模型
/// </summary>
public class FlowDebugRequest
{
    /// <summary>
    /// 流程输入参数字典
    /// Key: 参数名，Value: 参数值
    /// </summary>
    public Dictionary<string, object?> Params { get; set; } = new();
}