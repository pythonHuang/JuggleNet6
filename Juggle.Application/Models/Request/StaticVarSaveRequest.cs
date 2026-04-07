namespace Juggle.Application.Models.Request;

public class StaticVarSaveRequest
{
    public long?   Id           { get; set; }
    public string? VarCode      { get; set; }
    public string? VarName      { get; set; }
    public string? DataType     { get; set; }
    public string? Value        { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description  { get; set; }
    public string? GroupName    { get; set; }
}