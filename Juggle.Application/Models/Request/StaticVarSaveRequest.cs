namespace Juggle.Application.Models.Request;

/// <summary>
/// 保存静态变量请求模型
/// </summary>
public class StaticVarSaveRequest
{
    /// <summary>
    /// 变量 ID（编辑时传入，新增时为空）
    /// </summary>
    public long?   Id           { get; set; }

    /// <summary>
    /// 变量编码（唯一标识）
    /// </summary>
    public string? VarCode      { get; set; }

    /// <summary>
    /// 变量名称（中文描述）
    /// </summary>
    public string? VarName      { get; set; }

    /// <summary>
    /// 数据类型：string / integer / double / boolean / date / json
    /// </summary>
    public string? DataType     { get; set; }

    /// <summary>
    /// 当前值
    /// </summary>
    public string? Value        { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 变量描述
    /// </summary>
    public string? Description  { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string? GroupName    { get; set; }
}