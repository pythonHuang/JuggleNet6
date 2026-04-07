namespace Juggle.Application.Models.Request;

public class PageRequest
{
    public int PageNum { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
}