namespace Juggle.Application.Models.Request;

public class SuiteAddRequest
{
    public string SuiteName { get; set; } = "";
    public string? SuiteDesc { get; set; }
    public string? SuiteImage { get; set; }
    public string SuiteVersion { get; set; } = "v1.0.0";
}