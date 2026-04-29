namespace Juggle.Application.Models.Request;

/// <summary>
/// 参数保存请求
/// 用于保存接口或流程的参数定义
/// </summary>
public class ParameterSaveRequest
{
    /// <summary>
    /// 所属者 ID
    /// 如：API ID 或流程定义 ID
    /// </summary>
    public long OwnerId { get; set; }

    /// <summary>
    /// 所属者编码
    /// 如：API 的 methodCode
    /// </summary>
    public string OwnerCode { get; set; } = "";

    /// <summary>
    /// 参数类型
    /// 1-输入参数；2-输出参数
    /// </summary>
    public int ParamType { get; set; }

    /// <summary>
    /// 参数列表
    /// </summary>
    public List<ParameterItem> Parameters { get; set; } = new();
}