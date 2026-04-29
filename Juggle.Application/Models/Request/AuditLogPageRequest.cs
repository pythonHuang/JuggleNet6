namespace Juggle.Application.Models.Request;

/// <summary>
/// 审计日志分页查询请求
/// 继承自分页请求基类
/// </summary>
public class AuditLogPageRequest : PageRequest
{
    /// <summary>
    /// 模块名称（可选）
    /// 用于筛选指定模块的操作日志
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 操作类型（可选）
    /// 如：Add、Update、Delete、Query 等
    /// </summary>
    public string? ActionType { get; set; }
}