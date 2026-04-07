namespace Juggle.Application.Models.Response;

public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = "success";
    public T? Data { get; set; }

    public static ApiResult<T> Success(T data) => new() { Code = 200, Message = "success", Data = data };
    public static ApiResult<T> Fail(string message, int code = 500) => new() { Code = code, Message = message };
}

public class ApiResult
{
    public int Code { get; set; }
    public string Message { get; set; } = "success";
    public object? Data { get; set; }

    public static ApiResult Success(object? data = null) => new() { Code = 200, Message = "success", Data = data };
    public static ApiResult Fail(string message, int code = 500) => new() { Code = code, Message = message };
}
