namespace Juggle.Application.Models.Response;

public class PageResult<T>
{
    public long Total { get; set; }
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public List<T> Records { get; set; } = new();
}