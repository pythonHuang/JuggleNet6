using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/suite")]
[Authorize]
public class SuiteController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    public SuiteController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] SuiteAddRequest req)
    {
        var code = $"suite_{Guid.NewGuid():N}";
        var entity = new SuiteEntity
        {
            SuiteCode    = code,
            SuiteName    = req.SuiteName,
            SuiteDesc    = req.SuiteDesc,
            SuiteImage   = req.SuiteImage,
            SuiteVersion = req.SuiteVersion,
            SuiteFlag    = 0,
            TenantId     = _tenant.TenantId,
            CreatedAt    = DateTime.Now.ToString("o")
        };
        _db.Suites.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(entity.Id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Suites.FindAsync(id);
        if (entity == null) return ApiResult.Fail("套件不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] SuiteUpdateRequest req)
    {
        var entity = await _db.Suites.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("套件不存在");
        entity.SuiteName    = req.SuiteName;
        entity.SuiteDesc    = req.SuiteDesc;
        entity.SuiteImage   = req.SuiteImage;
        entity.SuiteVersion = req.SuiteVersion;
        entity.UpdatedAt    = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("info/{id}")]
    public async Task<ApiResult> Info(long id)
    {
        var entity = await _db.Suites.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("套件不存在");
        return ApiResult.Success(entity);
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] SuitePageRequest req)
    {
        var query = _db.Suites.Where(s => s.Deleted == 0);
        if (!string.IsNullOrEmpty(req.SuiteName))
            query = query.Where(s => s.SuiteName!.Contains(req.SuiteName));
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(s => s.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<SuiteEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpGet("list")]
    public async Task<ApiResult> List()
    {
        var list = await _db.Suites.Where(s => s.Deleted == 0).OrderBy(s => s.SuiteName).ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>导出套件（含所有接口、接口参数）为 JSON 文件</summary>
    [HttpGet("export/{id}")]
    public async Task<IActionResult> Export(long id)
    {
        var suite = await _db.Suites.FindAsync(id);
        if (suite == null || suite.Deleted == 1)
            return NotFound("套件不存在");

        var apis = await _db.Apis
            .Where(a => a.SuiteCode == suite.SuiteCode && a.Deleted == 0)
            .OrderBy(a => a.Id).ToListAsync();

        var apiIds = apis.Select(a => a.Id).ToList();
        var parameters = await _db.Parameters
            .Where(p => p.OwnerId != null && apiIds.Contains((long)p.OwnerId) && p.Deleted == 0)
            .OrderBy(p => p.OwnerId).ThenBy(p => p.SortNum).ToListAsync();

        var export = new
        {
            exportType   = "suite",
            exportTime   = DateTime.Now.ToString("o"),
            suiteCode    = suite.SuiteCode,
            suiteName    = suite.SuiteName,
            suiteDesc    = suite.SuiteDesc,
            suiteVersion = suite.SuiteVersion,
            suiteImage   = suite.SuiteImage,
            apis,
            parameters
        };

        var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        var fileName = $"suite_{suite.SuiteCode}_{DateTime.Now:yyyyMMddHHmmss}.json";
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    /// <summary>导入套件（含所有接口、接口参数），始终生成新的 suiteCode</summary>
    [HttpPost("import")]
    public async Task<ApiResult> Import([FromBody] System.Text.Json.JsonElement body)
    {
        try
        {
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!body.TryGetProperty("exportType", out var et) || et.GetString() != "suite")
                return ApiResult.Fail("无效的导入文件格式（exportType 应为 suite）");

            var oldSuiteCode  = body.TryGetProperty("suiteCode",    out var sc) ? sc.GetString()  : null;
            var suiteName     = body.TryGetProperty("suiteName",    out var sn) ? sn.GetString()  : "导入套件";
            var suiteDesc     = body.TryGetProperty("suiteDesc",    out var sd) ? sd.GetString()  : null;
            var suiteVersion  = body.TryGetProperty("suiteVersion", out var sv) ? sv.GetString()  : null;
            var suiteImage    = body.TryGetProperty("suiteImage",   out var si) ? si.GetString()  : null;

            // 生成新 suiteCode
            var newCode = $"suite_{Guid.NewGuid():N}";

            var entity = new SuiteEntity
            {
                SuiteCode    = newCode,
                SuiteName    = suiteName,
                SuiteDesc    = suiteDesc,
                SuiteVersion = suiteVersion,
                SuiteImage   = suiteImage,
                SuiteFlag    = 0,
                TenantId     = _tenant.TenantId,
                CreatedAt    = DateTime.Now.ToString("o")
            };
            _db.Suites.Add(entity);
            await _db.SaveChangesAsync();

            // 导入接口（建立旧 methodCode → 新 methodCode 的映射）
            var methodCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var oldApiIdMap   = new Dictionary<long, long>(); // 旧 api.Id → 新 api.Id

            if (body.TryGetProperty("apis", out var apisEl) && apisEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var importedApis = System.Text.Json.JsonSerializer.Deserialize<List<ApiEntity>>(apisEl.GetRawText(), opts) ?? new();
                foreach (var api in importedApis)
                {
                    var oldId         = api.Id;
                    var oldMethodCode = api.MethodCode;
                    var newMethodCode = $"api_{Guid.NewGuid():N}";

                    api.Id         = 0;
                    api.SuiteCode  = newCode;
                    api.MethodCode = newMethodCode;
                    api.Deleted    = 0;
                    api.CreatedAt  = DateTime.Now.ToString("o");
                    api.UpdatedAt  = null;
                    _db.Apis.Add(api);
                    await _db.SaveChangesAsync();  // 立即刷新获取新 Id

                    if (oldId > 0)
                        oldApiIdMap[oldId] = api.Id;
                    if (!string.IsNullOrEmpty(oldMethodCode))
                        methodCodeMap[oldMethodCode] = newMethodCode;
                }
            }

            // 导入参数（OwnerId 替换为新的 api.Id）
            if (body.TryGetProperty("parameters", out var paramsEl) && paramsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var pars = System.Text.Json.JsonSerializer.Deserialize<List<ParameterEntity>>(paramsEl.GetRawText(), opts) ?? new();
                foreach (var p in pars)
                {
                    if (p.OwnerId == null || !oldApiIdMap.TryGetValue(p.OwnerId.Value, out var newOwnerId))
                        continue;  // 找不到对应 api，跳过
                    p.Id        = 0;
                    p.OwnerId   = newOwnerId;
                    p.Deleted   = 0;
                    p.CreatedAt = DateTime.Now.ToString("o");
                    p.UpdatedAt = null;
                    _db.Parameters.Add(p);
                }
                await _db.SaveChangesAsync();
            }

            return ApiResult.Success(new { id = entity.Id, suiteCode = newCode, suiteName, methodCodeMap });
        }
        catch (Exception ex)
        {
            return ApiResult.Fail($"导入失败: {ex.Message}");
        }
    }
}
