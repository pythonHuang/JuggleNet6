namespace Juggle.Application.Models.Request;

public class ScheduleTaskAddRequest
{
    public string FlowKey { get; set; } = "";
    public string? FlowName { get; set; }
    public string CronExpression { get; set; } = "0 */5 * * * *";
    public string? InputJson { get; set; }
}