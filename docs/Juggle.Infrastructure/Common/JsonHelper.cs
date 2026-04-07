using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Juggle.Infrastructure.Common;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented        = false,
        Encoder              = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static string Serialize(object obj) => JsonSerializer.Serialize(obj, Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}

