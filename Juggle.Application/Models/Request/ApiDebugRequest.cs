namespace Juggle.Application.Models.Request;

/// <summary>
/// API 调试请求
/// 用于在界面上调试单个 API 的执行情况
/// </summary>
public class ApiDebugRequest
{
    /// <summary>
    /// API 记录 ID
    /// </summary>
    public long ApiId { get; set; }

    /// <summary>
    /// HTTP 请求头
    /// 键值对形式传递，如 Authorization、Content-Type 等
    /// </summary>
    public Dictionary<string, object?> Headers { get; set; } = new();

    /// <summary>
    /// HTTP 查询参数
    /// 键值对形式传递，GET 请求会拼接到 URL 后面
    /// </summary>
    public Dictionary<string, object?> Params { get; set; } = new();
}