using System.Security.Cryptography;
using System.Text;

namespace JuggleNet6.Backend.Infrastructure.Common;

public static class Md5Helper
{
    public static string Encrypt(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
