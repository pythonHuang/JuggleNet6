namespace Juggle.Domain.Entities;

/// <summary>
/// 流程变量信息实体
/// 定义流程的输入、输出和环境变量
/// </summary>
public class VariableInfoEntity : BaseEntity
{
    /// <summary>
    /// 关联的流程定义 ID
    /// </summary>
    public long? FlowDefinitionId { get; set; }

    /// <summary>
    /// 流程唯一标识 Key
    /// </summary>
    public string? FlowKey { get; set; }

    /// <summary>
    /// 变量编码（唯一标识）
    /// 流程中通过此编码引用变量
    /// </summary>
    public string? VariableCode { get; set; }

    /// <summary>
    /// 变量名称（中文描述）
    /// </summary>
    public string? VariableName { get; set; }

    /// <summary>
    /// 数据类型：
    /// - string：字符串
    /// - integer：整数
    /// - double：浮点数
    /// - boolean：布尔值
    /// - json：JSON 对象
    /// - array：数组
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// 变量类型：
    /// - input：输入变量（外部传入）
    /// - output：输出变量（流程产出）
    /// - env：环境变量（内部使用）
    /// </summary>
    public string? VariableType { get; set; }

    /// <summary>
    /// 默认值
    /// 当入参为空时使用此值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 变量描述说明
    /// </summary>
    public string? Description { get; set; }
}
