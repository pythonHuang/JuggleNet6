using JuggleNet6.Backend.Domain.Engine;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Open;

/// <summary>开放接口：供外部系统触发流程</summary>
[ApiController]
[Route("open/flow")]
public class FlowOpenController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly FlowEngine _flowEngine;

    public FlowOpenController(JuggleDbContext db, FlowEngine flowEngine)
    {
        _db = db;
        _flowEngine = flowEngine;
    }

    private async Task<bool> ValidateToken(string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        return await _db.Tokens.AnyAsync(t => t.TokenValue == token && t.Status == 1 && t.Deleted == 0);
    }

    [HttpGet("trigger/{version}/{key}")]
    public async Task<ApiResult> TriggerGet(string version, string key,
        [FromQuery] Dictionary<string, string> queryParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);

        return await TriggerFlow(version, key, queryParams.ToDictionary(k => k.Key, k => (object?)k.Value));
    }

    [HttpPost("trigger/{version}/{key}")]
    public async Task<ApiResult> TriggerPost(string version, string key,
        [FromBody] Dictionary<string, object?> bodyParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);

        return await TriggerFlow(version, key, bodyParams);
    }

    private async Task<ApiResult> TriggerFlow(string version, string key, Dictionary<string, object?> inputParams)
    {
        var flowVersion = await _db.FlowVersions
            .FirstOrDefaultAsync(v => v.FlowKey == key && v.Version == version && v.Status == 1 && v.Deleted == 0);
        if (flowVersion == null)
            return ApiResult.Fail("流程版本不存在或已禁用");

        var result = await _flowEngine.ExecuteAsync(flowVersion.FlowContent!, inputParams, key, version);
        return result.Success
            ? ApiResult.Success(result.OutputData)
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }
}
