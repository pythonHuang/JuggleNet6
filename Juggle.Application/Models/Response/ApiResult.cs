namespace Juggle.Application.Models.Response;

/// <summary>
/// 通用 API 响应封装类（泛型版本）
/// 统一规范接口返回值格式，便于前端统一处理
/// </summary>
/// <typeparam name="T">响应数据的类型</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 状态码，200 表示成功，其他值表示错误
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息，成功时为 "success"，失败时为错误描述
    /// </summary>
    public string Message { get; set; } = "success";

    /// <summary>
    /// 响应数据，类型为 T
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 返回成功响应
    /// </summary>
    /// <param name="data">要返回的数据</param>
    /// <returns>包装后的 ApiResult 对象</returns>
    public static ApiResult<T> Success(T data) => new() { Code = 200, Message = "success", Data = data };

    /// <summary>
    /// 返回失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码，默认为 500（服务器内部错误）</param>
    /// <returns>包装后的 ApiResult 对象</returns>
    public static ApiResult<T> Fail(string message, int code = 500) => new() { Code = code, Message = message };
}

/// <summary>
/// 通用 API 响应封装类（非泛型版本）
/// 用于不需要返回具体数据的场景
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 状态码，200 表示成功，其他值表示错误
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息，成功时为 "success"，失败时为错误描述
    /// </summary>
    public string Message { get; set; } = "success";

    /// <summary>
    /// 响应数据，可为任意类型
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 返回成功响应
    /// </summary>
    /// <param name="data">要返回的数据，默认为 null</param>
    /// <returns>包装后的 ApiResult 对象</returns>
    public static ApiResult Success(object? data = null) => new() { Code = 200, Message = "success", Data = data };

    /// <summary>
    /// 返回失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误状态码，默认为 500（服务器内部错误）</param>
    /// <returns>包装后的 ApiResult 对象</returns>
    public static ApiResult Fail(string message, int code = 500) => new() { Code = code, Message = message };
}
