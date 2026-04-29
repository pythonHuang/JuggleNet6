using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Juggle.Infrastructure.Common;

/// <summary>
/// JSON 序列化/反序列化帮助类
/// 提供统一的 JSON 处理方法，支持驼峰命名和完整的 Unicode 字符编码
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// JSON 序列化配置选项
    /// - PropertyNamingPolicy: 使用驼峰命名（camelCase）
    /// - WriteIndented: 不格式化输出（压缩为一行）
    /// - Encoder: 支持所有 Unicode 字符（包括中文等非 ASCII 字符不转义）
    /// </summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented        = false,
        Encoder              = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>JSON 字符串（驼峰命名，中文不转义）</returns>
    public static string Serialize(object obj) => JsonSerializer.Serialize(obj, Options);

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <returns>反序列化后的对象，如果解析失败返回 null</returns>
    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}

