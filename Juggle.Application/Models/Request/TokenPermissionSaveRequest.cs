namespace Juggle.Application.Models.Request;

/// <summary>
/// Token 权限保存请求
/// 用于为 Token 授权可访问的流程或 API
/// </summary>
public class TokenPermissionSaveRequest
{
    /// <summary>
    /// 权限类型
    /// FLOW-流程；API-接口
    /// </summary>
    public string PermissionType { get; set; } = "";

    /// <summary>
    /// 资源标识
    /// FLOW 类型时为 flowKey；API 类型时为 methodCode
    /// </summary>
    public string ResourceKey { get; set; } = "";

    /// <summary>
    /// 资源名称（可选）
    /// </summary>
    public string? ResourceName { get; set; }
}