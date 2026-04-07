namespace Juggle.Application.Models.Request;

public class ApiDebugRequest
{
    public long ApiId { get; set; }
    public Dictionary<string, object?> Headers { get; set; } = new();
    public Dictionary<string, object?> Params { get; set; } = new();
}