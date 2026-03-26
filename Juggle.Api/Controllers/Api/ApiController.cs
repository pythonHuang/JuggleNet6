using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/suite/api")]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiController(JuggleDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] ApiAddRequest req)
    {
        // 检查套件是否存在
        var suite = await _db.Suites.FirstOrDefaultAsync(s => s.SuiteCode == req.SuiteCode && s.Deleted == 0);
        if (suite == null) return ApiResult.Fail("套件不存在");

        var code = $"api_{Guid.NewGuid():N}";
        var entity = new ApiEntity
        {
            SuiteCode = req.SuiteCode,
            MethodCode = code,
            MethodName = req.MethodName,
            MethodDesc = req.MethodDesc,
            Url = req.Url,
            RequestType = req.RequestType,
            ContentType = req.ContentType,
            MockJson = req.MockJson,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.Apis.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(entity.Id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Apis.FindAsync(id);
        if (entity == null) return ApiResult.Fail("接口不存在");
        entity.Deleted = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] ApiUpdateRequest req)
    {
        var entity = await _db.Apis.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("接口不存在");
        entity.MethodName = req.MethodName;
        entity.MethodDesc = req.MethodDesc;
        entity.Url = req.Url;
        entity.RequestType = req.RequestType;
        entity.ContentType = req.ContentType;
        entity.MockJson = req.MockJson;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("info/{id}")]
    public async Task<ApiResult> Info(long id)
    {
        var entity = await _db.Apis.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("接口不存在");
        var inputParams = await _db.Parameters.Where(p => p.OwnerId == id && p.ParamType == 1 && p.Deleted == 0).OrderBy(p => p.SortNum).ToListAsync();
        var outputParams = await _db.Parameters.Where(p => p.OwnerId == id && p.ParamType == 2 && p.Deleted == 0).OrderBy(p => p.SortNum).ToListAsync();
        var headerParams = await _db.Parameters.Where(p => p.OwnerId == id && p.ParamType == 4 && p.Deleted == 0).OrderBy(p => p.SortNum).ToListAsync();
        return ApiResult.Success(new { api = entity, inputParams, outputParams, headerParams });
    }

    [HttpPost("list")]
    public async Task<ApiResult> List([FromBody] dynamic req)
    {
        string suiteCode = req.GetProperty("suiteCode").GetString() ?? "";
        var list = await _db.Apis.Where(a => a.SuiteCode == suiteCode && a.Deleted == 0).OrderBy(a => a.Id).ToListAsync();
        return ApiResult.Success(list);
    }

    [HttpPost("debug")]
    public async Task<ApiResult> Debug([FromBody] ApiDebugRequest req)
    {
        var api = await _db.Apis.FindAsync(req.ApiId);
        if (api == null) return ApiResult.Fail("接口不存在");

        var client = _httpClientFactory.CreateClient();

        foreach (var h in req.Headers)
            client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value?.ToString());

        try
        {
            string responseJson;
            var requestType = api.RequestType?.ToUpper() ?? "GET";

            if (requestType == "GET" || requestType == "DELETE")
            {
                var url = api.Url!;
                if (req.Params.Count > 0)
                {
                    var query = string.Join("&", req.Params.Select(kv =>
                        $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));
                    url = url.Contains('?') ? $"{url}&{query}" : $"{url}?{query}";
                }
                var resp = requestType == "GET" ? await client.GetAsync(url) : await client.DeleteAsync(url);
                responseJson = await resp.Content.ReadAsStringAsync();
            }
            else
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(req.Params),
                    System.Text.Encoding.UTF8, "application/json");
                var resp = requestType == "PUT" ? await client.PutAsync(api.Url, content) : await client.PostAsync(api.Url, content);
                responseJson = await resp.Content.ReadAsStringAsync();
            }

            return ApiResult.Success(new { response = responseJson });
        }
        catch (Exception ex)
        {
            return ApiResult.Fail($"调用失败: {ex.Message}");
        }
    }
}
