namespace Juggle.Application.Models.Request;

public class TokenAddRequest
{
    public string TokenName { get; set; } = "";
    public string? ExpiredAt { get; set; }
}