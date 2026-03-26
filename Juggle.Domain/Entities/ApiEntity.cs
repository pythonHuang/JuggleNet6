namespace Juggle.Domain.Entities;

public class ApiEntity : BaseEntity
{
    public string? SuiteCode { get; set; }
    public string? MethodCode { get; set; }
    public string? MethodName { get; set; }
    public string? MethodDesc { get; set; }
    public string? Url { get; set; }
    /// <summary>GET POST PUT DELETE</summary>
    public string? RequestType { get; set; }
    /// <summary>JSON FORM</summary>
    public string? ContentType { get; set; }
    public string? MockJson { get; set; }
}
