using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>
/// 流程日志控制器
/// 提供流程执行日志的查询、详情查看、删除、统计等功能
/// </summary>
[ApiController]
[Route("api/flow/log")]
[Authorize]
public class FlowLogController : ControllerBase
{
    private readonly JuggleDbContext _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public FlowLogController(JuggleDbContext db) => _db = db;

    /// <summary>
    /// 分页查询流程执行日志
    /// 支持按流程 Key、状态、时间范围筛选
    /// </summary>
    /// <param name="req">日志分页请求参数</param>
    /// <returns>流程执行日志列表</returns>
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

    /// <summary>查询异步流程执行结果（管理端，JWT认证）</summary>
    [HttpGet("asyncResult/{logId}")]
    public async Task<ApiResult> AsyncResult(long logId)
    {
        var log = await _db.FlowLogs.FirstOrDefaultAsync(l => l.Id == logId && l.Deleted == 0);
        if (log == null) return ApiResult.Fail("日志记录不存在");

        object? outputData = null;
        if (!string.IsNullOrEmpty(log.OutputJson) && log.Status != "RUNNING")
        {
            try { outputData = System.Text.Json.JsonSerializer.Deserialize<object>(log.OutputJson); }
            catch { outputData = log.OutputJson; }
        }

        return ApiResult.Success(new
        {
            logId    = log.Id,
            status   = log.Status,
            flowKey  = log.FlowKey,
            flowName = log.FlowName,
            version  = log.Version,
            startTime = log.StartTime,
            endTime  = log.EndTime,
            costMs   = log.CostMs,
            errorMessage = log.ErrorMessage,
            output   = outputData
        });
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

    /// <summary>仪表盘统计：今日执行次数/成功率/平均耗时 + 近7天趋势</summary>
    [HttpGet("dashboard")]
    public async Task<ApiResult> Dashboard()
    {
        var today = DateTime.Today.ToString("o");
        var sevenDaysAgo = DateTime.Now.AddDays(-7).ToString("o");

        // 今日统计
        var todayLogs = await _db.FlowLogs
            .Where(l => l.Deleted == 0 && l.StartTime != null && l.StartTime.CompareTo(today) >= 0)
            .ToListAsync();
        var todayTotal = todayLogs.Count;
        var todaySuccess = todayLogs.Count(l => l.Status == "SUCCESS");
        var todayFailed = todayLogs.Count(l => l.Status == "FAILED");
        var avgCostMs = todayTotal > 0 ? (long)todayLogs.Where(l => l.CostMs > 0).Average(l => l.CostMs) : 0;

        // 总体统计
        var totalLogs = await _db.FlowLogs.Where(l => l.Deleted == 0).CountAsync();

        // 最近10次失败流程
        var recentFailed = await _db.FlowLogs
            .Where(l => l.Deleted == 0 && l.Status == "FAILED")
            .OrderByDescending(l => l.Id)
            .Take(10)
            .Select(l => new { l.Id, l.FlowKey, l.FlowName, l.ErrorMessage, l.StartTime })
            .ToListAsync();

        // 近7天执行趋势（按天分组）
        var weekLogs = await _db.FlowLogs
            .Where(l => l.Deleted == 0 && l.StartTime != null && l.StartTime.CompareTo(sevenDaysAgo) >= 0)
            .ToListAsync();
        var weekTrend = weekLogs
            .GroupBy(l => l.StartTime?.Substring(0, 10) ?? "")
            .Select(g => new { date = g.Key, total = g.Count(), success = g.Count(l => l.Status == "SUCCESS"), failed = g.Count(l => l.Status == "FAILED") })
            .OrderBy(x => x.date)
            .ToList();

        // Top 5 流程执行次数
        var topFlows = await _db.FlowLogs
            .Where(l => l.Deleted == 0)
            .GroupBy(l => new { l.FlowKey, l.FlowName })
            .Select(g => new { flowKey = g.Key.FlowKey, flowName = g.Key.FlowName, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        return ApiResult.Success(new
        {
            today = new { total = todayTotal, success = todaySuccess, failed = todayFailed, avgCostMs, successRate = todayTotal > 0 ? Math.Round((double)todaySuccess / todayTotal * 100, 1) : 0 },
            totalLogs,
            recentFailed,
            weekTrend,
            topFlows
        });
    }
}
