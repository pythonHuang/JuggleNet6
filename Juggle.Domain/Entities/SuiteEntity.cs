namespace Juggle.Domain.Entities;

/// <summary>
/// 套件实体
/// 用于组织和封装一组相关的 API 方法
/// </summary>
public class SuiteEntity : BaseEntity
{
    /// <summary>
    /// 套件编码（唯一标识）
    /// </summary>
    public string? SuiteCode { get; set; }

    /// <summary>
    /// 套件名称
    /// </summary>
    public string? SuiteName { get; set; }

    /// <summary>
    /// 套件分类 ID
    /// </summary>
    public long? SuiteClassifyId { get; set; }

    /// <summary>
    /// 套件图标（URL 或 Base64）
    /// </summary>
    public string? SuiteImage { get; set; }

    /// <summary>
    /// 套件版本号
    /// </summary>
    public string? SuiteVersion { get; set; }

    /// <summary>
    /// 套件描述
    /// </summary>
    public string? SuiteDesc { get; set; }

    /// <summary>
    /// 套件帮助文档（JSON 格式）
    /// </summary>
    public string? SuiteHelpDocJson { get; set; }

    /// <summary>
    /// 套件标识：
    /// - 0：自定义套件（用户创建）
    /// - 1：官方套件（预置）
    /// </summary>
    public int SuiteFlag { get; set; } = 0;
}
