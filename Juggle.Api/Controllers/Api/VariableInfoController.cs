using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/flow/variable")]
[Authorize]
public class VariableInfoController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public VariableInfoController(JuggleDbContext db) => _db = db;

    [HttpGet("list/{flowKey}")]
    public async Task<ApiResult> List(string flowKey)
    {
        var list = await _db.VariableInfos
            .Where(v => v.FlowKey == flowKey && v.Deleted == 0)
            .OrderBy(v => v.Id)
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>保存变量列表（全量替换）</summary>
    [HttpPost("save")]
    public async Task<ApiResult> Save([FromBody] JsonElement req)
    {
        var flowKey          = req.GetProperty("flowKey").GetString() ?? "";
        var flowDefinitionId = req.GetProperty("flowDefinitionId").GetInt64();
        
        // 使用与全局配置一致的JSON序列化选项（camelCase）
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        var variables    = req.GetProperty("variables").Deserialize<List<VariableInfoEntity>>(jsonOptions)
                           ?? new List<VariableInfoEntity>();

        // 删除旧变量
        var old = await _db.VariableInfos.Where(v => v.FlowKey == flowKey && v.Deleted == 0).ToListAsync();
        foreach (var v in old) { v.Deleted = 1; v.UpdatedAt = DateTime.Now.ToString("o"); }

        // 添加新变量
        foreach (var item in variables)
        {
            _db.VariableInfos.Add(new VariableInfoEntity
            {
                FlowDefinitionId = flowDefinitionId,
                FlowKey          = flowKey,
                VariableCode     = item.VariableCode,
                VariableName     = item.VariableName,
                DataType         = item.DataType,
                VariableType     = item.VariableType,
                DefaultValue     = item.DefaultValue,
                Description      = item.Description,
                CreatedAt        = DateTime.Now.ToString("o")
            });
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
