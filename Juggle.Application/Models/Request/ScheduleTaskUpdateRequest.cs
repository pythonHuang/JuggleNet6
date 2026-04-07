namespace Juggle.Application.Models.Request;

public class ScheduleTaskUpdateRequest : ScheduleTaskAddRequest
{
    public long Id { get; set; }
    public int Status { get; set; }
}