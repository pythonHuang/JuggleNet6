using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Response;
using Juggle.Application.Services.Flow;
using Juggle.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Open;

/// <summary>开放接口：供外部系统通过 Access Token 触发流程执行。</summary>
[ApiController]
[Route("open/flow")]
public class FlowOpenController : ControllerBase
{
    private readonly JuggleDbContext      _db;
    private readonly FlowExecutionService _flowExec;

    public FlowOpenController(JuggleDbContext db, FlowExecutionService flowExec)
    {
        _db       = db;
        _flowExec = flowExec;
    }

    private async Task<bool> ValidateToken(string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        return await _db.Tokens.AnyAsync(t => t.TokenValue == token && t.Status == 1 && t.Deleted == 0);
    }

    /// <summary>校验 Token 对指定流程是否有权限（无权限配置则允许全部）</summary>
    private async Task<bool> ValidateFlowPermission(string? token, string flowKey)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var tokenEntity = await _db.Tokens.FirstOrDefaultAsync(t => t.TokenValue == token && t.Status == 1 && t.Deleted == 0);
        if (tokenEntity == null) return false;

        // 查询是否有权限配置
        var hasPermissions = await _db.TokenPermissions.AnyAsync(p => p.TokenId == tokenEntity.Id && p.Deleted == 0);
        if (!hasPermissions) return true; // 无权限配置 = 全部允许

        // 有权限配置，检查是否包含该流程
        return await _db.TokenPermissions.AnyAsync(p =>
            p.TokenId == tokenEntity.Id && p.PermissionType == "FLOW" &&
            p.ResourceKey == flowKey && p.Deleted == 0);
    }

    // ──────────────────────────────────────────────
    // 带版本号
    // ──────────────────────────────────────────────

    [HttpGet("trigger/{version}/{key}")]
    public async Task<ApiResult> TriggerGet(string version, string key,
        [FromQuery] Dictionary<string, string> queryParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);
        if (!await ValidateFlowPermission(token, key))
            return ApiResult.Fail("该 Token 无权访问此流程");

        return await TriggerFlowByVersion(version, key,
            queryParams.ToDictionary(k => k.Key, k => (object?)k.Value));
    }

    [HttpPost("trigger/{version}/{key}")]
    public async Task<ApiResult> TriggerPost(string version, string key,
        [FromBody] Dictionary<string, object?> bodyParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);
        if (!await ValidateFlowPermission(token, key))
            return ApiResult.Fail("该 Token 无权访问此流程");

        return await TriggerFlowByVersion(version, key, bodyParams);
    }

    // ──────────────────────────────────────────────
    // 不带版本号（取最新已发布版本）
    // ──────────────────────────────────────────────

    [HttpGet("trigger/{key}")]
    public async Task<ApiResult> TriggerLatestGet(string key,
        [FromQuery] Dictionary<string, string> queryParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);
        if (!await ValidateFlowPermission(token, key))
            return ApiResult.Fail("该 Token 无权访问此流程");

        return await TriggerFlowLatest(key,
            queryParams.ToDictionary(k => k.Key, k => (object?)k.Value));
    }

    [HttpPost("trigger/{key}")]
    public async Task<ApiResult> TriggerLatestPost(string key,
        [FromBody] Dictionary<string, object?> bodyParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);
        if (!await ValidateFlowPermission(token, key))
            return ApiResult.Fail("该 Token 无权访问此流程");

        return await TriggerFlowLatest(key, bodyParams);
    }

    // ──────────────────────────────────────────────
    // 异步触发（立即返回 logId，后台执行）
    // ──────────────────────────────────────────────

    /// <summary>
    /// 异步触发流程（POST），立即返回 logId，不等待执行完成。
    /// 调用方通过 GET /open/flow/result/{logId} 轮询执行结果。
    /// </summary>
    [HttpPost("triggerAsync/{key}")]
    public async Task<ApiResult> TriggerAsyncPost(string key,
        [FromBody] Dictionary<string, object?> bodyParams,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);
        if (!await ValidateFlowPermission(token, key))
            return ApiResult.Fail("该 Token 无权访问此流程");

        var flowVersion = await _db.FlowVersions
            .Where(v => v.FlowKey == key && v.Status == 1 && v.Deleted == 0)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync();
        if (flowVersion == null)
            return ApiResult.Fail("未找到已发布的流程版本");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == key && f.Deleted == 0);
        if (definition == null)
            return ApiResult.Fail("流程定义不存在");

        // 预先写入一条 RUNNING 日志，获得 logId
        var logId = await _flowExec.CreateRunningLogAsync(definition, flowVersion.Version!, bodyParams);

        // 后台异步执行，不阻塞当前请求
        _ = Task.Run(async () =>
        {
            try
            {
                await _flowExec.RunAsyncWithLog(definition, flowVersion.FlowContent!, bodyParams,
                    "open_async", flowVersion.Version!, logId);
            }
            catch { /* 异常已在 RunAsyncWithLog 内记录到日志 */ }
        });

        return ApiResult.Success(new { logId, message = "流程已提交异步执行，请通过 logId 轮询结果" });
    }

    /// <summary>
    /// 查询异步流程执行结果。
    /// status: RUNNING（执行中）/ SUCCESS（成功）/ FAILED（失败）
    /// </summary>
    [HttpGet("result/{logId}")]
    public async Task<ApiResult> GetAsyncResult(long logId,
        [FromHeader(Name = "X-Access-Token")] string? token)
    {
        if (!await ValidateToken(token))
            return ApiResult.Fail("无效的 Access Token", 401);

        var log = await _db.FlowLogs.FirstOrDefaultAsync(l => l.Id == logId && l.Deleted == 0);
        if (log == null)
            return ApiResult.Fail("日志记录不存在");

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

    // ──────────────────────────────────────────────
    // 内部实现
    // ──────────────────────────────────────────────

    private async Task<ApiResult> TriggerFlowByVersion(
        string version, string key, Dictionary<string, object?> inputParams)
    {
        var flowVersion = await _db.FlowVersions
            .FirstOrDefaultAsync(v => v.FlowKey == key && v.Version == version
                                   && v.Status == 1 && v.Deleted == 0);
        if (flowVersion == null)
            return ApiResult.Fail("流程版本不存在或已禁用");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == key && f.Deleted == 0);
        if (definition == null)
            return ApiResult.Fail("流程定义不存在");

        var result = await _flowExec.RunAsync(
            definition, flowVersion.FlowContent!, inputParams, "open", version);

        return result.Success
            ? ApiResult.Success(result.OutputData)
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }

    private async Task<ApiResult> TriggerFlowLatest(
        string key, Dictionary<string, object?> inputParams)
    {
        // 取最新已发布（status=1）版本
        var flowVersion = await _db.FlowVersions
            .Where(v => v.FlowKey == key && v.Status == 1 && v.Deleted == 0)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync();

        if (flowVersion == null)
            return ApiResult.Fail("未找到已发布的流程版本");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == key && f.Deleted == 0);
        if (definition == null)
            return ApiResult.Fail("流程定义不存在");

        var result = await _flowExec.RunAsync(
            definition, flowVersion.FlowContent!, inputParams, "open", flowVersion.Version!);

        return result.Success
            ? ApiResult.Success(result.OutputData)
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }
}

