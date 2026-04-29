using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services.Flow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>
/// 流程版本管理控制器
/// 提供流程版本的查询、状态管理、手动触发执行等功能
/// </summary>
[ApiController]
[Route("api/flow/version")]
[Authorize]
public class FlowVersionController : ControllerBase
{
    private readonly JuggleDbContext      _db;
    private readonly FlowExecutionService _flowExec;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="flowExec">流程执行服务</param>
    public FlowVersionController(JuggleDbContext db, FlowExecutionService flowExec)
    {
        _db       = db;
        _flowExec = flowExec;
    }

    /// <summary>
    /// 分页查询流程版本列表
    /// </summary>
    /// <param name="req">版本分页请求参数</param>
    /// <returns>流程版本列表</returns>
    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] FlowVersionPageRequest req)
    {
        var query = _db.FlowVersions.Where(v => v.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowKey))
            query = query.Where(v => v.FlowKey == req.FlowKey);
        var total   = await query.CountAsync();
        var records = await query
            .OrderByDescending(v => v.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<Juggle.Domain.Entities.FlowVersionEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpPut("status")]
    public async Task<ApiResult> UpdateStatus([FromBody] FlowVersionStatusRequest req)
    {
        var entity = await _db.FlowVersions.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("版本不存在");
        entity.Status    = req.Status;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.FlowVersions.FindAsync(id);
        if (entity == null) return ApiResult.Fail("版本不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("latest/{flowKey}")]
    public async Task<ApiResult> Latest(string flowKey)
    {
        var version = await _db.FlowVersions
            .Where(v => v.FlowKey == flowKey && v.Status == 1 && v.Deleted == 0)
            .OrderByDescending(v => v.Id)
            .Select(v => v.Version)
            .FirstOrDefaultAsync();
        return ApiResult.Success(version);
    }

    /// <summary>通过管理接口触发流程（测试用）— 完整记录日志并回写静态变量。</summary>
    [HttpPost("trigger/{version}/{key}")]
    public async Task<ApiResult> Trigger(string version, string key, [FromBody] FlowDebugRequest req)
    {
        var flowVersion = await _db.FlowVersions
            .FirstOrDefaultAsync(v => v.FlowKey == key && v.Version == version
                                   && v.Status == 1 && v.Deleted == 0);
        if (flowVersion == null) return ApiResult.Fail("流程版本不存在或已禁用");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == key && f.Deleted == 0);
        if (definition == null) return ApiResult.Fail("流程定义不存在");

        var result = await _flowExec.RunAsync(
            definition, flowVersion.FlowContent!, req.Params, "version", version);

        return result.Success
            ? ApiResult.Success(new { outputs = result.OutputData, logId = result.LogId, costMs = result.CostMs })
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }
}
