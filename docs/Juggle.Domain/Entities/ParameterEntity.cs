namespace Juggle.Domain.Entities;

public class ParameterEntity : BaseEntity
{
    public long? OwnerId { get; set; }
    public string? OwnerCode { get; set; }
    /// <summary>1:API入参 2:API出参 3:对象属性 4:API Header 5:流程入参 6:流程出参</summary>
    public int ParamType { get; set; }
    public string? ParamCode { get; set; }
    public string? ParamName { get; set; }
    public string? DataType { get; set; }  // string int boolean object array
    public string? ObjectCode { get; set; }  // 当DataType=object时引用的对象code
    public int Required { get; set; } = 0;
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
    public int SortNum { get; set; } = 0;
}
