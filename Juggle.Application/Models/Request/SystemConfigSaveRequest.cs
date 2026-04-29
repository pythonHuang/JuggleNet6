namespace Juggle.Application.Models.Request;

/// <summary>
/// 系统配置保存请求
/// </summary>
public class SystemConfigSaveRequest
{
    /// <summary>
    /// 配置键
    /// </summary>
    public string ConfigKey { get; set; } = "";

    /// <summary>
    /// 配置值（可选）
    /// </summary>
    public string? ConfigValue { get; set; }
}