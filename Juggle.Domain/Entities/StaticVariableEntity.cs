namespace Juggle.Domain.Entities;

/// <summary>
/// 静态（全局）变量定义实体
/// 可在任意流程中引用和修改，执行结束后自动回写到数据库
/// 流程中通过 $static.{VarCode} 语法引用
/// </summary>
public class StaticVariableEntity : BaseEntity
{
    /// <summary>
    /// 变量编码（唯一键）
    /// 流程中通过 $static.{VarCode} 语法引用
    /// </summary>
    public string? VarCode { get; set; }

    /// <summary>
    /// 变量名称（中文描述，便于理解）
    /// </summary>
    public string? VarName { get; set; }

    /// <summary>
    /// 数据类型：
    /// - string：字符串
    /// - integer：整数
    /// - double：浮点数
    /// - boolean：布尔值
    /// - date：日期
    /// - json：JSON 对象
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// 当前值（以字符串形式存储）
    /// 流程执行中被修改后会回写到数据库
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 默认值
    /// 当 Value 为空时使用此值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 变量描述说明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分组名称（可按业务模块分组管理）
    /// </summary>
    public string? GroupName { get; set; }
}
