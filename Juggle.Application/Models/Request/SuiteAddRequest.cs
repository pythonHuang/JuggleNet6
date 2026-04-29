namespace Juggle.Application.Models.Request;

/// <summary>
/// 测试套件新增请求
/// 用于创建包含多个测试用例的套件
/// </summary>
public class SuiteAddRequest
{
    /// <summary>
    /// 测试套件名称
    /// </summary>
    public string SuiteName { get; set; } = "";

    /// <summary>
    /// 测试套件描述（可选）
    /// </summary>
    public string? SuiteDesc { get; set; }

    /// <summary>
    /// 套件图标（可选）
    /// Base64 编码或 URL
    /// </summary>
    public string? SuiteImage { get; set; }

    /// <summary>
    /// 套件版本号
    /// </summary>
    public string SuiteVersion { get; set; } = "v1.0.0";
}