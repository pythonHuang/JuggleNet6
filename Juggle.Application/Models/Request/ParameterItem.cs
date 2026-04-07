namespace Juggle.Application.Models.Request;

public class ParameterItem
{
    public long? Id { get; set; }
    public string ParamCode { get; set; } = "";
    public string ParamName { get; set; } = "";
    public string DataType { get; set; } = "string";
    public string? ObjectCode { get; set; }
    public int Required { get; set; } = 0;
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public int SortNum { get; set; }
}