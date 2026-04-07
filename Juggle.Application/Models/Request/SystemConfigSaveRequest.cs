namespace Juggle.Application.Models.Request;

public class SystemConfigSaveRequest
{
    public string ConfigKey { get; set; } = "";
    public string? ConfigValue { get; set; }
}