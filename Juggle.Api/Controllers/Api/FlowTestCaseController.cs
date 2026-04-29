using System.Text.Json;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services.Flow;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>
/// 流程测试用例控制器
/// 提供测试用例的增删改查、批量执行、断言校验等功能
/// </summary>
[ApiController]
[Route("api/flow/testcase")]
[Authorize]
public class FlowTestCaseController : ControllerBase
{
    private readonly JuggleDbContext    _db;
    private readonly FlowExecutionService _flowExec;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="flowExec">流程执行服务</param>
    public FlowTestCaseController(JuggleDbContext db, FlowExecutionService flowExec)
    {
        _db       = db;
        _flowExec = flowExec;
    }

    /// <summary>
    /// 分页查询测试用例列表
    /// </summary>
    /// <param name="req">测试用例分页请求参数</param>
    /// <returns>测试用例列表</returns>
    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] FlowTestCasePageRequest req)
    {
        var query = _db.FlowTestCases.Where(c => c.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowKey))
            query = query.Where(c => c.FlowKey == req.FlowKey);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(c => c.CaseName.Contains(req.Keyword));
        var total   = await query.CountAsync();
        var records = await query.OrderByDescending(c => c.Id)
            .Skip((req.PageNum - 1) * req.PageSize).Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new { total, records });
    }

    [HttpPost("save")]
    public async Task<ApiResult> Save([FromBody] FlowTestCaseSaveRequest req)
    {
        if (req.Id.HasValue && req.Id > 0)
        {
            var entity = await _db.FlowTestCases.FindAsync(req.Id.Value);
            if (entity == null) return ApiResult.Fail("用例不存在");
            entity.CaseName  = req.CaseName;
            entity.InputJson = req.InputJson;
            entity.AssertJson= req.AssertJson;
            entity.Remark    = req.Remark;
            entity.UpdatedAt = DateTime.Now.ToString("o");
        }
        else
        {
            _db.FlowTestCases.Add(new FlowTestCaseEntity
            {
                FlowKey       = req.FlowKey,
                CaseName      = req.CaseName,
                InputJson     = req.InputJson,
                AssertJson    = req.AssertJson,
                LastRunStatus = "PENDING",
                Remark        = req.Remark,
                Deleted       = 0,
                CreatedAt     = DateTime.Now.ToString("o")
            });
        }
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.FlowTestCases.FindAsync(id);
        if (entity == null) return ApiResult.Fail("用例不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>执行测试用例并进行断言校验</summary>
    [HttpPost("run/{id}")]
    public async Task<ApiResult> Run(long id)
    {
        var testCase = await _db.FlowTestCases.FindAsync(id);
        if (testCase == null || testCase.Deleted == 1) return ApiResult.Fail("用例不存在");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(d => d.FlowKey == testCase.FlowKey && d.Deleted == 0);
        if (definition == null) return ApiResult.Fail("流程不存在");
        if (string.IsNullOrEmpty(definition.FlowContent) || definition.FlowContent == "[]")
            return ApiResult.Fail("流程内容为空");

        // 解析入参
        Dictionary<string, object?> inputParams = new();
        if (!string.IsNullOrEmpty(testCase.InputJson))
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                inputParams = JsonSerializer.Deserialize<Dictionary<string, object?>>(testCase.InputJson, opts) ?? new();
            }
            catch { }
        }

        // 执行流程
        var execResult = await _flowExec.RunAsync(definition, definition.FlowContent!, inputParams, "testcase");

        // 断言校验
        var assertResults = new List<object>();
        bool allPassed = true;

        if (!string.IsNullOrEmpty(testCase.AssertJson) && execResult.Success)
        {
            try
            {
                var asserts = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(testCase.AssertJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                foreach (var (varName, expectedVal) in asserts)
                {
                    string? expected = expectedVal.ValueKind == JsonValueKind.String
                        ? expectedVal.GetString()
                        : expectedVal.GetRawText();

                    string? actual = null;
                    if (execResult.OutputData?.TryGetValue(varName, out var actualObj) == true)
                        actual = actualObj?.ToString();

                    bool passed = actual == expected;
                    if (!passed) allPassed = false;
                    assertResults.Add(new { varName, expected, actual, passed });
                }
            }
            catch (Exception ex)
            {
                assertResults.Add(new { varName = "_parse_error", expected = "", actual = ex.Message, passed = false });
                allPassed = false;
            }
        }

        var finalStatus = execResult.Success && allPassed ? "SUCCESS" : "FAILED";
        var summary = execResult.Success
            ? (assertResults.Count > 0 ? $"断言 {assertResults.Count(a => ((dynamic)a).passed)} / {assertResults.Count} 通过" : "执行成功（无断言）")
            : $"执行失败: {execResult.ErrorMessage}";

        // 更新用例状态
        testCase.LastRunStatus = finalStatus;
        testCase.LastRunTime   = DateTime.Now.ToString("o");
        testCase.LastRunResult = summary;
        testCase.UpdatedAt     = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();

        return ApiResult.Success(new
        {
            success        = execResult.Success && allPassed,
            status         = finalStatus,
            summary,
            errorMessage   = execResult.ErrorMessage,
            outputs        = execResult.OutputData,
            assertResults,
            logId          = execResult.LogId,
            costMs         = execResult.CostMs
        });
    }

    /// <summary>批量执行某个流程的所有测试用例</summary>
    [HttpPost("runAll/{flowKey}")]
    public async Task<ApiResult> RunAll(string flowKey)
    {
        var cases = await _db.FlowTestCases
            .Where(c => c.FlowKey == flowKey && c.Deleted == 0)
            .ToListAsync();

        if (cases.Count == 0) return ApiResult.Fail("该流程暂无测试用例");

        var results = new List<object>();
        int passCount = 0;

        foreach (var tc in cases)
        {
            // 复用 Run 逻辑（简化版，直接内联）
            var definition = await _db.FlowDefinitions
                .FirstOrDefaultAsync(d => d.FlowKey == tc.FlowKey && d.Deleted == 0);
            if (definition == null || string.IsNullOrEmpty(definition.FlowContent) || definition.FlowContent == "[]")
            {
                results.Add(new { caseId = tc.Id, caseName = tc.CaseName, status = "FAILED", summary = "流程不存在或内容为空" });
                continue;
            }

            Dictionary<string, object?> inputParams = new();
            if (!string.IsNullOrEmpty(tc.InputJson))
            {
                try { inputParams = JsonSerializer.Deserialize<Dictionary<string, object?>>(tc.InputJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new(); } catch { }
            }

            var execResult = await _flowExec.RunAsync(definition, definition.FlowContent!, inputParams, "testcase");
            bool allPassed = execResult.Success;

            if (execResult.Success && !string.IsNullOrEmpty(tc.AssertJson))
            {
                try
                {
                    var asserts = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tc.AssertJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    foreach (var (varName, expectedVal) in asserts)
                    {
                        string? expected = expectedVal.ValueKind == JsonValueKind.String ? expectedVal.GetString() : expectedVal.GetRawText();
                        object? actualObj = null;
                        execResult.OutputData?.TryGetValue(varName, out actualObj);
                        string? actual = actualObj?.ToString();
                        if (actual != expected) { allPassed = false; break; }
                    }
                }
                catch { allPassed = false; }
            }

            var status  = allPassed ? "SUCCESS" : "FAILED";
            var summary = execResult.Success ? (allPassed ? "断言全部通过" : "断言失败") : $"执行失败: {execResult.ErrorMessage}";
            if (allPassed) passCount++;

            tc.LastRunStatus = status;
            tc.LastRunTime   = DateTime.Now.ToString("o");
            tc.LastRunResult = summary;
            tc.UpdatedAt     = DateTime.Now.ToString("o");
            results.Add(new { caseId = tc.Id, caseName = tc.CaseName, status, summary });
        }
        await _db.SaveChangesAsync();

        return ApiResult.Success(new
        {
            total = cases.Count,
            passCount,
            failCount = cases.Count - passCount,
            results
        });
    }
}
