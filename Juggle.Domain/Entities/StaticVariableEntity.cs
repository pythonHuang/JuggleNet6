namespace Juggle.Domain.Entities;

/// <summary>静态（全局）变量定义 —— 可在任意流程中引用和修改</summary>
public class StaticVariableEntity : BaseEntity
{
    /// <summary>变量编码（唯一键，流程中用 $static.xxx 引用）</summary>
    public string? VarCode { get; set; }
    /// <summary>变量名称（中文描述）</summary>
    public string? VarName { get; set; }
    /// <summary>数据类型：string / integer / double / boolean / json</summary>
    public string? DataType { get; set; }
    /// <summary>当前值（字符串形式存储）</summary>
    public string? Value { get; set; }
    /// <summary>默认值</summary>
    public string? DefaultValue { get; set; }
    /// <summary>描述</summary>
    public string? Description { get; set; }
    /// <summary>分组（可按业务分组）</summary>
    public string? GroupName { get; set; }
}
