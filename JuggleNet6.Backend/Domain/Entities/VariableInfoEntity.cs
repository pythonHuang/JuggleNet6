namespace JuggleNet6.Backend.Domain.Entities;

public class VariableInfoEntity : BaseEntity
{
    public long? FlowDefinitionId { get; set; }
    public string? FlowKey { get; set; }
    public string? VariableCode { get; set; }
    public string? VariableName { get; set; }
    public string? DataType { get; set; }
    /// <summary>input/output/env</summary>
    public string? VariableType { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
}
