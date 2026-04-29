namespace Juggle.Domain.Entities;

/// <summary>
/// 对象定义实体
/// 用于定义 API 请求/响应的复杂对象结构
/// </summary>
public class ObjectEntity : BaseEntity
{
    /// <summary>
    /// 对象编码（唯一标识）
    /// </summary>
    public string? ObjectCode { get; set; }

    /// <summary>
    /// 对象名称（中文描述）
    /// </summary>
    public string? ObjectName { get; set; }

    /// <summary>
    /// 对象描述说明
    /// </summary>
    public string? ObjectDesc { get; set; }
}
