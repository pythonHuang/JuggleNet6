namespace JuggleNet6.Backend.Domain.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public int Deleted { get; set; } = 0;
    public string? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public string? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}
