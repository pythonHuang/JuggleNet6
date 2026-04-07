namespace Juggle.Application.Models.Request;

public class ScheduleTaskPageRequest : PageRequest
{
    public string? FlowKey { get; set; }
    public int? Status { get; set; }
}