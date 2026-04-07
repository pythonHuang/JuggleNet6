namespace Juggle.Domain.Entities;

/// <summary>系统配置项（键值对）</summary>
public class SystemConfigEntity : BaseEntity
{
    public string ConfigKey { get; set; } = "";       // 配置键，全局唯一
    public string? ConfigValue { get; set; }           // 配置值
    public string? ConfigName { get; set; }            // 显示名称
    public string? ConfigGroup { get; set; }           // 分组（email/alert/system）
    public string? Remark { get; set; }
}
