namespace Juggle.Application.Models.Request;

/// <summary>
/// 设置值请求
/// 用于流程执行过程中修改变量的值
/// </summary>
public class SetValueRequest
{
    /// <summary>
    /// 要设置的值
    /// </summary>
    public string? Value { get; set; }
}