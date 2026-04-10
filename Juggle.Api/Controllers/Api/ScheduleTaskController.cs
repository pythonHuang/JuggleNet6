using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Api.Services;
using Juggle.Application.Services;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleTaskController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    public ScheduleTaskController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] ScheduleTaskAddRequest req)
    {
        // 查找流程名称
        var flow = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == req.FlowKey && f.Deleted == 0);
        if (flow == null) return ApiResult.Fail("流程不存在");

        var entity = new ScheduleTaskEntity
        {
            FlowKey        = req.FlowKey,
            FlowName       = req.FlowName ?? flow.FlowName,
            CronExpression = req.CronExpression,
            InputJson      = req.InputJson,
            Status         = 1,
            TenantId       = _tenant.TenantId,
            NextRunTime    = ScheduleTaskService.CalculateNextRun(req.CronExpression, DateTime.Now),
            CreatedAt      = DateTime.Now.ToString("o")
        };
        _db.ScheduleTasks.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = entity.Id });
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] ScheduleTaskUpdateRequest req)
    {
        var entity = await _db.ScheduleTasks.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("任务不存在");

        entity.FlowKey        = req.FlowKey;
        entity.FlowName       = req.FlowName;
        entity.CronExpression = req.CronExpression;
        entity.InputJson      = req.InputJson;
        entity.Status         = req.Status;
        entity.NextRunTime    = req.Status == 1
            ? ScheduleTaskService.CalculateNextRun(req.CronExpression, DateTime.Now)
            : null;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.ScheduleTasks.FindAsync(id);
        if (entity == null) return ApiResult.Fail("任务不存在");
        entity.Deleted = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPost("toggle/{id}")]
    public async Task<ApiResult> Toggle(long id)
    {
        var entity = await _db.ScheduleTasks.FindAsync(id);
        if (entity == null) return ApiResult.Fail("任务不存在");
        entity.Status = entity.Status == 1 ? 0 : 1;
        entity.NextRunTime = entity.Status == 1
            ? ScheduleTaskService.CalculateNextRun(entity.CronExpression, DateTime.Now)
            : null;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { status = entity.Status });
    }

    [HttpPost("runNow/{id}")]
    public async Task<ApiResult> RunNow(long id)
    {
        var entity = await _db.ScheduleTasks.FindAsync(id);
        if (entity == null) return ApiResult.Fail("任务不存在");

        // 立即将 nextRunTime 设为过去，让调度器在下一分钟执行
        entity.NextRunTime = DateTime.Now.AddSeconds(-1);
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { message = "任务将在1分钟内执行" });
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] ScheduleTaskPageRequest req)
    {
        var query = _db.ScheduleTasks.Where(t => t.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowKey))
            query = query.Where(t => t.FlowKey == req.FlowKey);
        if (req.Status.HasValue)
            query = query.Where(t => t.Status == req.Status.Value);

        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(t => t.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<ScheduleTaskEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }
}
