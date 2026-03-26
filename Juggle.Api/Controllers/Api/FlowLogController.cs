using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/flow/log")]
[Authorize]
public class FlowLogController : ControllerBase
{
    private readonly JuggleDbContext _db;
    public FlowLogController(JuggleDbContext db) => _db = db;

    /// <summary>分页查询流程执行日志</summary>
    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] FlowLogPageRequest req)
    {
        var query = _db.FlowLogs.Where(l => l.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowKey))
            query = query.Where(l => l.FlowKey == req.FlowKey);
        if (!string.IsNullOrEmpty(req.Status))
            query = query.Where(l => l.Status == req.Status);
        if (!string.IsNullOrEmpty(req.StartDate))
            query = query.Where(l => l.StartTime != null && l.StartTime.CompareTo(req.StartDate) >= 0);
        if (!string.IsNullOrEmpty(req.EndDate))
            query = query.Where(l => l.StartTime != null && l.StartTime.CompareTo(req.EndDate + "T99") <= 0);

        var total   = await query.CountAsync();
        var records = await query
            .OrderByDescending(l => l.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return ApiResult.Success(new PageResult<object>
        {
            Total    = total,
            PageNum  = req.PageNum,
            PageSize = req.PageSize,
            Records  = records.Cast<object>().ToList()
        });
    }

    /// <summary>查询某条日志的节点明细</summary>
    [HttpGet("detail/{logId}")]
    public async Task<ApiResult> Detail(long logId)
    {
        var log = await _db.FlowLogs.FindAsync(logId);
        if (log == null || log.Deleted == 1) return ApiResult.Fail("日志不存在");

        var nodeLogs = await _db.FlowNodeLogs
            .Where(n => n.FlowLogId == logId && n.Deleted == 0)
            .OrderBy(n => n.SeqNo)
            .ToListAsync();

        return ApiResult.Success(new { log, nodeLogs });
    }

    /// <summary>删除日志</summary>
    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var log = await _db.FlowLogs.FindAsync(id);
        if (log == null) return ApiResult.Fail("日志不存在");
        log.Deleted   = 1;
        log.UpdatedAt = DateTime.Now.ToString("o");
        var nodeLogs = await _db.FlowNodeLogs.Where(n => n.FlowLogId == id).ToListAsync();
        nodeLogs.ForEach(n => { n.Deleted = 1; n.UpdatedAt = DateTime.Now.ToString("o"); });
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>清空某流程的所有日志</summary>
    [HttpDelete("clear/{flowKey}")]
    public async Task<ApiResult> Clear(string flowKey)
    {
        var logs   = await _db.FlowLogs.Where(l => l.FlowKey == flowKey && l.Deleted == 0).ToListAsync();
        var logIds = logs.Select(l => l.Id).ToList();
        logs.ForEach(l => { l.Deleted = 1; l.UpdatedAt = DateTime.Now.ToString("o"); });

        if (logIds.Count > 0)
        {
            var nodeLogs = await _db.FlowNodeLogs
                .Where(n => logIds.Contains(n.FlowLogId!.Value) && n.Deleted == 0)
                .ToListAsync();
            nodeLogs.ForEach(n => { n.Deleted = 1; n.UpdatedAt = DateTime.Now.ToString("o"); });
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
