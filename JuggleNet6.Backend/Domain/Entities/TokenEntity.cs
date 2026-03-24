namespace JuggleNet6.Backend.Domain.Entities;

public class TokenEntity : BaseEntity
{
    public string? TokenValue { get; set; }
    public string? TokenName { get; set; }
    public string? ExpiredAt { get; set; }
    public int Status { get; set; } = 1;
}
