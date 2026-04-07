namespace Juggle.Application.Models.Request;

public class ApiAddRequest
{
    public string SuiteCode { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string? MethodDesc { get; set; }
    public string Url { get; set; } = "";
    public string RequestType { get; set; } = "GET";
    public string ContentType { get; set; } = "JSON";
    public string? MockJson { get; set; }
    /// <summary>HTTP WEBSERVICE</summary>
    public string MethodType { get; set; } = "HTTP";
}