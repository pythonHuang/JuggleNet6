using JuggleNet6.Backend.Domain.Engine;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Api;

[ApiController]
[Route("api/flow/version")]
[Authorize]
public class FlowVersionController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly FlowEngine _flowEngine;

    public FlowVersionController(JuggleDbContext db, FlowEngine flowEngine)
    {
        _db = db;
        _flowEngine = flowEngine;
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] FlowVersionPageRequest req)
    {
        var query = _db.FlowVersions.Where(v => v.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowKey))
            query = query.Where(v => v.FlowKey == req.FlowKey);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(v => v.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<Domain.Entities.FlowVersionEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpPut("status")]
    public async Task<ApiResult> UpdateStatus([FromBody] FlowVersionStatusRequest req)
    {
        var entity = await _db.FlowVersions.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("版本不存在");
        entity.Status = req.Status;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.FlowVersions.FindAsync(id);
        if (entity == null) return ApiResult.Fail("版本不存在");
        entity.Deleted = 1;
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

    /// <summary>通过管理接口触发流程（测试用）</summary>
    [HttpPost("trigger/{version}/{key}")]
    public async Task<ApiResult> Trigger(string version, string key, [FromBody] FlowDebugRequest req)
    {
        var flowVersion = await _db.FlowVersions
            .FirstOrDefaultAsync(v => v.FlowKey == key && v.Version == version && v.Status == 1 && v.Deleted == 0);
        if (flowVersion == null) return ApiResult.Fail("流程版本不存在或已禁用");

        var result = await _flowEngine.ExecuteAsync(flowVersion.FlowContent!, req.Params, key, version);
        return result.Success
            ? ApiResult.Success(result.OutputData)
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }
}
