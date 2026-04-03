using System.Collections.Concurrent;
using System.Text.Json;
using Juggle.Application.Services.Flow;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Services;

/// <summary>
/// 轻量级定时任务调度器：每分钟扫描一次 t_schedule_task 表，
/// 对 status=1 且到达 next_run_time 的任务触发流程执行。
/// </summary>
public class ScheduleTaskService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduleTaskService> _logger;
    private readonly ConcurrentDictionary<long, bool> _runningTasks = new();

    public ScheduleTaskService(IServiceProvider serviceProvider, ILogger<ScheduleTaskService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("定时任务调度器已启动");
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "定时任务调度器执行异常"); }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task TickAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JuggleDbContext>();
        var flowExec = scope.ServiceProvider.GetRequiredService<FlowExecutionService>();

        var now = DateTime.Now;
        var tasks = await db.ScheduleTasks
            .Where(t => t.Deleted == 0 && t.Status == 1 && t.NextRunTime != null && t.NextRunTime <= now)
            .ToListAsync();

        foreach (var task in tasks)
        {
            if (!_runningTasks.TryAdd(task.Id, true)) continue;
            try
            {
                _logger.LogInformation("定时任务触发: {TaskId} FlowKey={FlowKey}", task.Id, task.FlowKey);

                var definition = await db.FlowDefinitions
                    .FirstOrDefaultAsync(f => f.FlowKey == task.FlowKey && f.Deleted == 0);
                if (definition == null) continue;

                var version = await db.FlowVersions
                    .Where(v => v.FlowKey == task.FlowKey && v.Status == 1 && v.Deleted == 0)
                    .OrderByDescending(v => v.Id).FirstOrDefaultAsync();
                if (version == null) continue;

                Dictionary<string, object?> inputParams = new();
                if (!string.IsNullOrEmpty(task.InputJson))
                {
                    try { inputParams = JsonSerializer.Deserialize<Dictionary<string, object?>>(task.InputJson) ?? new(); }
                    catch { }
                }

                var startTime = DateTime.Now;
                var result = await flowExec.RunAsync(definition, version.FlowContent!, inputParams, "schedule", version.Version!);

                task.LastRunTime = startTime;
                task.LastRunStatus = result.Success ? "SUCCESS" : "FAILED";
                task.RunCount++;
                task.NextRunTime = CalculateNextRun(task.CronExpression, now);
                task.UpdatedAt = now.ToString("o");
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务 {TaskId} 执行异常", task.Id);
                try
                {
                    task.LastRunTime = DateTime.Now;
                    task.LastRunStatus = "FAILED";
                    task.NextRunTime = CalculateNextRun(task.CronExpression, DateTime.Now);
                    task.UpdatedAt = DateTime.Now.ToString("o");
                    await db.SaveChangesAsync();
                }
                catch { }
            }
            finally { _runningTasks.TryRemove(task.Id, out _); }
        }
    }

    /// <summary>简单 Cron 解析：支持6位/5位格式（秒 分 时 日 月 周），计算下次运行时间</summary>
    public static DateTime? CalculateNextRun(string? cronExpression, DateTime after)
    {
        if (string.IsNullOrEmpty(cronExpression)) return null;
        try
        {
            var parts = cronExpression.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return null;

            var minutePart = parts.Length > 1 ? parts[1] : "*";
            var hourPart = parts.Length > 2 ? parts[2] : "*";

            var next = after.AddMinutes(1);
            next = new DateTime(next.Year, next.Month, next.Day, next.Hour, 0, 0);

            int maxIterations = 1440;
            while (maxIterations-- > 0)
            {
                if (MatchCronField(minutePart, next.Minute) && MatchCronField(hourPart, next.Hour))
                    return next;
                next = next.AddMinutes(1);
            }
            return null;
        }
        catch { return null; }
    }

    private static bool MatchCronField(string field, int value)
    {
        if (field == "*") return true;
        if (field.StartsWith("*/"))
        {
            if (int.TryParse(field[2..], out var interval) && interval > 0)
                return value % interval == 0;
        }
        if (int.TryParse(field, out var num)) return value == num;
        return true;
    }
}
