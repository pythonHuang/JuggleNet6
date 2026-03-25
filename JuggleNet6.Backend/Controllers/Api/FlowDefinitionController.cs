using JuggleNet6.Backend.Domain.Entities;
using JuggleNet6.Backend.Domain.Engine;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Api;

[ApiController]
[Route("api/flow/definition")]
[Authorize]
public class FlowDefinitionController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly FlowEngine _flowEngine;

    public FlowDefinitionController(JuggleDbContext db, FlowEngine flowEngine)
    {
        _db = db;
        _flowEngine = flowEngine;
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] FlowDefinitionAddRequest req)
    {
        var key = $"flow_{Guid.NewGuid():N}";
        var entity = new FlowDefinitionEntity
        {
            FlowKey = key,
            FlowName = req.FlowName,
            FlowDesc = req.FlowDesc,
            FlowType = req.FlowType,
            FlowContent = "[]",
            Status = 0,
            CreatedAt = DateTime.Now.ToString("o")
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
        entity.Deleted = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] FlowDefinitionUpdateRequest req)
    {
        var entity = await _db.FlowDefinitions.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("流程不存在");
        entity.FlowName = req.FlowName;
        entity.FlowDesc = req.FlowDesc;
        entity.FlowType = req.FlowType;
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
        entity.UpdatedAt = DateTime.Now.ToString("o");
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

    /// <summary>调试流程</summary>
    [HttpPost("debug/{flowKey}")]
    public async Task<ApiResult> Debug(string flowKey, [FromBody] FlowDebugRequest req,
        [FromServices] IHttpClientFactory httpClientFactory)
    {
        var entity = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == flowKey && f.Deleted == 0);
        if (entity == null) return ApiResult.Fail("流程不存在");
        if (string.IsNullOrEmpty(entity.FlowContent) || entity.FlowContent == "[]")
            return ApiResult.Fail("流程内容为空，请先设计流程");

        // 加载数据源连接字符串（供 MYSQL 节点使用）
        var dsConnStrings = await BuildDataSourceConnStrings();
        var engine = new FlowEngine(httpClientFactory, dsConnStrings);
        var result = await engine.ExecuteAsync(entity.FlowContent, req.Params, flowKey, "debug");
        return result.Success
            ? ApiResult.Success(result.OutputData)
            : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
    }

    /// <summary>构建数据源名称→连接字符串映射</summary>
    private async Task<Dictionary<string, string>> BuildDataSourceConnStrings()
    {
        var dataSources = await _db.DataSources.Where(d => d.Deleted == 0).ToListAsync();
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ds in dataSources)
        {
            if (string.IsNullOrEmpty(ds.DsName)) continue;
            string connStr;
            if (string.Equals(ds.DsType, "sqlite", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(ds.Host))
            {
                // SQLite：DbName 当作文件路径
                var dbPath = string.IsNullOrEmpty(ds.DbName) ? "juggle.db" : ds.DbName;
                connStr = $"Data Source={dbPath}";
            }
            else
            {
                // MySQL
                connStr = $"Server={ds.Host};Port={ds.Port};Database={ds.DbName};User={ds.Username};Password={ds.Password};";
            }
            map[ds.DsName] = connStr;
        }
        return map;
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
                FlowKey = definition.FlowKey,
                FlowName = definition.FlowName,
                FlowDesc = definition.FlowDesc,
                FlowType = definition.FlowType,
                Status = 1,
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
            int.TryParse(latestVersion[1..], out versionNum) ;
        var newVersion = $"v{versionNum + (latestVersion != null ? 1 : 0)}";
        if (latestVersion == null) newVersion = "v1";

        var flowVersion = new FlowVersionEntity
        {
            FlowInfoId = flowInfo.Id,
            FlowKey = definition.FlowKey,
            Version = newVersion,
            FlowContent = definition.FlowContent,
            Status = 1,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.FlowVersions.Add(flowVersion);
        definition.Status = 1;
        await _db.SaveChangesAsync();

        return ApiResult.Success(new { version = newVersion });
    }
}
