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
            MethodType = req.MethodType,
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
        entity.MethodType = req.MethodType;
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

        var methodType = api.MethodType?.ToUpper() ?? "HTTP";

        try
        {
            // WebService（SOAP 1.1）调试
            if (methodType == "WEBSERVICE")
            {
                return await DebugWebService(api, req);
            }

            // HTTP 调试（原有逻辑）
            var client = _httpClientFactory.CreateClient();
            foreach (var h in req.Headers)
                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value?.ToString());

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

    /// <summary>调试 WebService（SOAP 1.1）接口</summary>
    private async Task<ApiResult> DebugWebService(ApiEntity api, ApiDebugRequest req)
    {
        // 从 Url 中提取 soapAction（可通过 Header "SOAPAction" 传入，也可为空）
        var soapAction = req.Headers.ContainsKey("SOAPAction") ? req.Headers["SOAPAction"]?.ToString() ?? "" : "";

        // 构建 SOAP 1.1 请求体：用入参 key/value 组装 XML 参数
        var paramXml = string.Join("", req.Params.Select(kv =>
            $"<{kv.Key}>{System.Security.SecurityElement.Escape(kv.Value?.ToString() ?? "")}</{kv.Key}>"));

        // 从 Url 中解析 method name（取路径最后一段，例如 http://ws/HelloService?op=SayHello → SayHello）
        var methodName = "Request";
        var uri = new Uri(api.Url!);
        // 解析 ?op=MethodName 参数
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        if (queryParams.TryGetValue("op", out var opVal) && !string.IsNullOrEmpty(opVal))
            methodName = opVal!;

        var soapBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <{methodName} xmlns=""{uri.GetLeftPart(UriPartial.Path)}"">
      {paramXml}
    </{methodName}>
  </soap:Body>
</soap:Envelope>";

        var client = _httpClientFactory.CreateClient();
        // SOAP 1.1 必须 POST，Content-Type: text/xml
        var content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml");
        if (!string.IsNullOrEmpty(soapAction))
            content.Headers.Add("SOAPAction", $"\"{soapAction}\"");

        // 附加其他 headers
        foreach (var h in req.Headers)
            if (h.Key != "SOAPAction")
                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value?.ToString());

        var resp = await client.PostAsync(api.Url, content);
        var responseText = await resp.Content.ReadAsStringAsync();
        return ApiResult.Success(new { response = responseText, soapBody });
    }
}
