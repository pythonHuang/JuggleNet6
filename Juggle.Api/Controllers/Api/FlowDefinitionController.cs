using System.Text.Json;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Juggle.Application.Services.Flow;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>
/// 流程定义控制器
/// 提供流程的增删改查、调试、部署、导入导出等功能
/// </summary>
[ApiController]
[Route("api/flow/definition")]
[Authorize]
public class FlowDefinitionController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly FlowExecutionService _flowExec;
    private readonly ITenantAccessor _tenant;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="flowExec">流程执行服务</param>
    /// <param name="tenant">多租户访问器</param>
    public FlowDefinitionController(JuggleDbContext db, FlowExecutionService flowExec, ITenantAccessor tenant)
    {
        _db       = db;
        _flowExec = flowExec;
        _tenant   = tenant;
    }

    /// <summary>
    /// 新增流程定义
    /// </summary>
    /// <param name="req">流程基本信息（名称、描述、类型、分组）</param>
    /// <returns>新创建的流程 ID 和 FlowKey</returns>
    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] FlowDefinitionAddRequest req)
    {
        var key = $"flow_{Guid.NewGuid():N}";
        var entity = new FlowDefinitionEntity
        {
            FlowKey     = key,
            FlowName    = req.FlowName,
            FlowDesc    = req.FlowDesc,
            FlowType    = req.FlowType,
            GroupName   = req.GroupName,
            FlowContent = "[]",
            Status      = 0,
            TenantId    = _tenant.TenantId,
            CreatedAt   = DateTime.Now.ToString("o")
        };
        _db.FlowDefinitions.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = entity.Id, flowKey = entity.FlowKey });
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.FlowDefinitions.FindAsync(id);
        if (entity == null) return ApiResult.Fail("流程不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] FlowDefinitionUpdateRequest req)
    {
        var entity = await _db.FlowDefinitions.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("流程不存在");
        entity.FlowName  = req.FlowName;
        entity.FlowDesc  = req.FlowDesc;
        entity.FlowType  = req.FlowType;
        entity.GroupName = req.GroupName;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>保存流程内容（节点JSON）</summary>
    [HttpPut("save")]
    public async Task<ApiResult> Save([FromBody] FlowDefinitionSaveRequest req)
    {
        var entity = await _db.FlowDefinitions.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("流程不存在");
        entity.FlowContent = req.FlowContent;
        entity.UpdatedAt   = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("infoByKey/{flowKey}")]
    public async Task<ApiResult> InfoByKey(string flowKey)
    {
        var entity = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == flowKey && f.Deleted == 0);
        if (entity == null) return ApiResult.Fail("流程不存在");
        var variables = await _db.VariableInfos
            .Where(v => v.FlowKey == flowKey && v.Deleted == 0).ToListAsync();
        var inputParams = await _db.Parameters
            .Where(p => p.OwnerId == entity.Id && p.ParamType == 5 && p.Deleted == 0)
            .OrderBy(p => p.SortNum).ToListAsync();
        var outputParams = await _db.Parameters
            .Where(p => p.OwnerId == entity.Id && p.ParamType == 6 && p.Deleted == 0)
            .OrderBy(p => p.SortNum).ToListAsync();
        return ApiResult.Success(new { definition = entity, variables, inputParams, outputParams });
    }

    [HttpGet("info/{id}")]
    public async Task<ApiResult> Info(long id)
    {
        var entity = await _db.FlowDefinitions.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("流程不存在");
        var variables = await _db.VariableInfos
            .Where(v => v.FlowDefinitionId == id && v.Deleted == 0)
            .ToListAsync();
        var inputParams = await _db.Parameters
            .Where(p => p.OwnerId == id && p.ParamType == 5 && p.Deleted == 0)
            .OrderBy(p => p.SortNum).ToListAsync();
        var outputParams = await _db.Parameters
            .Where(p => p.OwnerId == id && p.ParamType == 6 && p.Deleted == 0)
            .OrderBy(p => p.SortNum).ToListAsync();
        return ApiResult.Success(new { definition = entity, variables, inputParams, outputParams });
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] FlowDefinitionPageRequest req)
    {
        var query = _db.FlowDefinitions.Where(f => f.Deleted == 0);
        if (!string.IsNullOrEmpty(req.FlowName))
            query = query.Where(f => f.FlowName!.Contains(req.FlowName));
        if (!string.IsNullOrEmpty(req.GroupName))
            query = query.Where(f => f.GroupName == req.GroupName);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(f => f.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<FlowDefinitionEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpGet("groups")]
    public async Task<ApiResult> Groups()
    {
        var groups = await _db.FlowDefinitions
            .Where(f => f.Deleted == 0 && f.GroupName != null && f.GroupName != "")
            .Select(f => f.GroupName!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
        return ApiResult.Success(groups);
    }

    /// <summary>调试流程</summary>
    [HttpPost("debug/{flowKey}")]
    public async Task<ApiResult> Debug(string flowKey, [FromBody] FlowDebugRequest req)
    {
        var entity = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == flowKey && f.Deleted == 0);
        if (entity == null) return ApiResult.Fail("流程不存在");
        if (string.IsNullOrEmpty(entity.FlowContent) || entity.FlowContent == "[]")
            return ApiResult.Fail("流程内容为空，请先设计流程");

        var result = await _flowExec.RunAsync(entity, entity.FlowContent!, req.Params, "debug");

        // 查询节点执行明细（无论成功还是失败都查，用于前端高亮报错节点）
        List<FlowNodeLogEntity> nodeLogs = new();
        if (result.LogId.HasValue)
        {
            nodeLogs = await _db.FlowNodeLogs
                .Where(n => n.FlowLogId == result.LogId.Value && n.Deleted == 0)
                .OrderBy(n => n.SeqNo)
                .ToListAsync();
        }

        // 即使失败也返回 200 + 详细节点日志，让前端高亮报错节点
        return ApiResult.Success(new
        {
            success = result.Success,
            errorMessage = result.ErrorMessage,
            outputs = result.OutputData,
            logId = result.LogId,
            costMs = result.CostMs,
            nodeLogs = nodeLogs.Select(n => new
            {
                nodeKey = n.NodeKey,
                nodeType = n.NodeType,
                status = n.Status,
                executionTime = n.CostMs,
                detail = n.Detail,
                errorMessage = n.ErrorMessage,
                inputSnapshot = n.InputSnapshot,
                outputSnapshot = n.OutputSnapshot,
                startTime = n.StartTime,
                endTime = n.EndTime
            }).ToList()
        });
    }

    /// <summary>导出流程定义（含变量、参数）为 JSON 文件</summary>
    [HttpGet("export/{id}")]
    public async Task<IActionResult> Export(long id)
    {
        var entity = await _db.FlowDefinitions.FindAsync(id);
        if (entity == null || entity.Deleted == 1)
            return NotFound("流程不存在");

        var variables = await _db.VariableInfos
            .Where(v => v.FlowDefinitionId == id && v.Deleted == 0).ToListAsync();
        var parameters = await _db.Parameters
            .Where(p => p.OwnerId == id && (p.ParamType == 5 || p.ParamType == 6) && p.Deleted == 0)
            .OrderBy(p => p.SortNum).ToListAsync();

        var export = new
        {
            exportType  = "flow",
            exportTime  = DateTime.Now.ToString("o"),
            flowKey     = entity.FlowKey,
            flowName    = entity.FlowName,
            flowDesc    = entity.FlowDesc,
            flowType    = entity.FlowType,
            flowContent = entity.FlowContent,
            variables,
            parameters
        };

        var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        var fileName = $"flow_{entity.FlowKey}_{DateTime.Now:yyyyMMddHHmmss}.json";
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    /// <summary>导入流程定义（含变量、参数），相同 flowKey 时自动重命名</summary>
    [HttpPost("import")]
    public async Task<ApiResult> Import([FromBody] System.Text.Json.JsonElement body)
    {
        try
        {
            var opts = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!body.TryGetProperty("exportType", out var et) || et.GetString() != "flow")
                return ApiResult.Fail("无效的导入文件格式（exportType 应为 flow）");

            var flowName    = body.GetProperty("flowName").GetString() ?? "导入流程";
            var flowDesc    = body.TryGetProperty("flowDesc", out var fd) ? fd.GetString() : null;
            var flowType    = body.TryGetProperty("flowType", out var fty) ? fty.GetString() : null;
            var flowContent = body.TryGetProperty("flowContent", out var fc) ? fc.GetString() : "[]";

            // 生成新的 flowKey（不复用原来的，避免冲突）
            var newKey = $"flow_{Guid.NewGuid():N}";
            var entity = new FlowDefinitionEntity
            {
                FlowKey     = newKey,
                FlowName    = flowName,
                FlowDesc    = flowDesc,
                FlowType    = flowType,
                FlowContent = flowContent,
                Status      = 0,
                TenantId    = _tenant.TenantId,
                CreatedAt   = DateTime.Now.ToString("o")
            };
            _db.FlowDefinitions.Add(entity);
            await _db.SaveChangesAsync();

            // 导入变量
            if (body.TryGetProperty("variables", out var varsEl) && varsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var vars = System.Text.Json.JsonSerializer.Deserialize<List<VariableInfoEntity>>(varsEl.GetRawText(), opts) ?? new();
                foreach (var v in vars)
                {
                    v.Id               = 0;
                    v.FlowDefinitionId = entity.Id;
                    v.FlowKey          = newKey;
                    v.Deleted          = 0;
                    v.CreatedAt        = DateTime.Now.ToString("o");
                    v.UpdatedAt        = null;
                    _db.VariableInfos.Add(v);
                }
            }

            // 导入参数
            if (body.TryGetProperty("parameters", out var paramsEl) && paramsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var pars = System.Text.Json.JsonSerializer.Deserialize<List<ParameterEntity>>(paramsEl.GetRawText(), opts) ?? new();
                foreach (var p in pars)
                {
                    p.Id        = 0;
                    p.OwnerId   = entity.Id;
                    p.Deleted   = 0;
                    p.CreatedAt = DateTime.Now.ToString("o");
                    p.UpdatedAt = null;
                    _db.Parameters.Add(p);
                }
            }

            await _db.SaveChangesAsync();
            return ApiResult.Success(new { id = entity.Id, flowKey = newKey, flowName });
        }
        catch (Exception ex)
        {
            return ApiResult.Fail($"导入失败: {ex.Message}");
        }
    }

    /// <summary>克隆/复制流程定义（含变量、参数）</summary>
    [HttpPost("clone/{id}")]
    public async Task<ApiResult> Clone(long id)
    {
        var source = await _db.FlowDefinitions.FindAsync(id);
        if (source == null || source.Deleted == 1) return ApiResult.Fail("流程不存在");

        var newKey = $"flow_{Guid.NewGuid():N}";
        var cloned = new FlowDefinitionEntity
        {
            FlowKey     = newKey,
            FlowName    = $"{source.FlowName}_副本",
            FlowDesc    = source.FlowDesc,
            FlowType    = source.FlowType,
            GroupName   = source.GroupName,
            FlowContent = source.FlowContent,
            Status      = 0,
            TenantId    = _tenant.TenantId,
            CreatedAt   = DateTime.Now.ToString("o")
        };
        _db.FlowDefinitions.Add(cloned);
        await _db.SaveChangesAsync();

        // 复制变量
        var vars = await _db.VariableInfos
            .Where(v => v.FlowDefinitionId == id && v.Deleted == 0).ToListAsync();
        foreach (var v in vars)
        {
            _db.VariableInfos.Add(new VariableInfoEntity
            {
                FlowDefinitionId = cloned.Id,
                FlowKey          = newKey,
                VariableCode     = v.VariableCode,
                VariableName     = v.VariableName,
                DataType         = v.DataType,
                VariableType     = v.VariableType,
                DefaultValue     = v.DefaultValue,
                Description      = v.Description,
                Deleted          = 0,
                CreatedAt        = DateTime.Now.ToString("o")
            });
        }

        // 复制参数（入参/出参）
        var pars = await _db.Parameters
            .Where(p => p.OwnerId == id && (p.ParamType == 5 || p.ParamType == 6) && p.Deleted == 0)
            .ToListAsync();
        foreach (var p in pars)
        {
            _db.Parameters.Add(new ParameterEntity
            {
                OwnerId      = cloned.Id,
                OwnerCode    = newKey,
                ParamType    = p.ParamType,
                ParamCode    = p.ParamCode,
                ParamName    = p.ParamName,
                DataType     = p.DataType,
                ObjectCode   = p.ObjectCode,
                Required     = p.Required,
                DefaultValue = p.DefaultValue,
                Description  = p.Description,
                SortNum      = p.SortNum,
                Deleted      = 0,
                CreatedAt    = DateTime.Now.ToString("o")
            });
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = cloned.Id, flowKey = newKey, flowName = cloned.FlowName });
    }

    /// <summary>部署流程</summary>
    [HttpPost("deploy")]
    public async Task<ApiResult> Deploy([FromBody] FlowDeployRequest req)
    {
        var definition = await _db.FlowDefinitions.FindAsync(req.FlowDefinitionId);
        if (definition == null) return ApiResult.Fail("流程不存在");
        if (string.IsNullOrEmpty(definition.FlowContent) || definition.FlowContent == "[]")
            return ApiResult.Fail("流程内容为空，无法部署");

        // 获取或创建 FlowInfo
        var flowInfo = await _db.FlowInfos
            .FirstOrDefaultAsync(fi => fi.FlowKey == definition.FlowKey && fi.Deleted == 0);
        if (flowInfo == null)
        {
            flowInfo = new FlowInfoEntity
            {
                FlowKey   = definition.FlowKey,
                FlowName  = definition.FlowName,
                FlowDesc  = definition.FlowDesc,
                FlowType  = definition.FlowType,
                Status    = 1,
                TenantId  = _tenant.TenantId,
                CreatedAt = DateTime.Now.ToString("o")
            };
            _db.FlowInfos.Add(flowInfo);
            await _db.SaveChangesAsync();
        }

        // 计算版本号
        var latestVersion = await _db.FlowVersions
            .Where(v => v.FlowKey == definition.FlowKey && v.Deleted == 0)
            .OrderByDescending(v => v.Id)
            .Select(v => v.Version)
            .FirstOrDefaultAsync();

        int versionNum = 1;
        if (latestVersion != null && latestVersion.StartsWith("v"))
            int.TryParse(latestVersion[1..], out versionNum);
        var newVersion = latestVersion == null ? "v1" : $"v{versionNum + 1}";

        var flowVersion = new FlowVersionEntity
        {
            FlowInfoId  = flowInfo.Id,
            FlowKey     = definition.FlowKey,
            Version     = newVersion,
            FlowContent = definition.FlowContent,
            Status      = 1,
            TenantId    = _tenant.TenantId,
            CreatedAt   = DateTime.Now.ToString("o")
        };
        _db.FlowVersions.Add(flowVersion);
        definition.Status = 1;
        await _db.SaveChangesAsync();

        return ApiResult.Success(new { version = newVersion });
    }
}
