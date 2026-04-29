namespace Juggle.Domain.Entities;

/// <summary>
/// 系统配置项实体（键值对存储）
/// 用于存储系统的各种配置参数，如邮件配置、告警配置、系统参数等
/// </summary>
public class SystemConfigEntity : BaseEntity
{
    /// <summary>
    /// 配置键（全局唯一）
    /// 程序通过此键读取配置
    /// </summary>
    public string ConfigKey { get; set; } = "";

    /// <summary>
    /// 配置值
    /// </summary>
    public string? ConfigValue { get; set; }

    /// <summary>
    /// 配置显示名称（中文描述）
    /// </summary>
    public string? ConfigName { get; set; }

    /// <summary>
    /// 配置分组
    /// 用于配置分类管理，例如：
    /// - email：邮件配置
    /// - alert：告警配置
    /// - system：系统配置
    /// </summary>
    public string? ConfigGroup { get; set; }

    /// <summary>
    /// 配置描述说明
    /// </summary>
    public string? Remark { get; set; }
}
