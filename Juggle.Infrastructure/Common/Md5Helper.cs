using System.Security.Cryptography;
using System.Text;

namespace Juggle.Infrastructure.Common;

/// <summary>
/// MD5 加密帮助类
/// 提供字符串的 MD5 哈希计算功能
/// </summary>
public static class Md5Helper
{
    /// <summary>
    /// 计算字符串的 MD5 哈希值
    /// </summary>
    /// <param name="input">要加密的原始字符串</param>
    /// <returns>32位小写十六进制格式的 MD5 哈希值</returns>
    /// <remarks>
    /// 注意：MD5 已被证明存在安全漏洞，不适合用于密码存储等安全场景。
    /// 此方法主要用于数据完整性校验或旧系统兼容。
    /// </remarks>
    public static string Encrypt(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
