namespace Juggle.Application.Models.Request;

public class ParameterSaveRequest
{
    public long OwnerId { get; set; }
    public string OwnerCode { get; set; } = "";
    public int ParamType { get; set; }
    public List<ParameterItem> Parameters { get; set; } = new();
}