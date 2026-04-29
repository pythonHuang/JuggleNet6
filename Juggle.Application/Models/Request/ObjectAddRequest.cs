namespace Juggle.Application.Models.Request;

/// <summary>
/// 业务对象新增请求
/// 用于创建流程中使用的业务对象实体
/// </summary>
public class ObjectAddRequest
{
    /// <summary>
    /// 业务对象名称
    /// </summary>
    public string ObjectName { get; set; } = "";

    /// <summary>
    /// 业务对象描述（可选）
    /// </summary>
    public string? ObjectDesc { get; set; }
}