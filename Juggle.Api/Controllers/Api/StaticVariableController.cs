using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>静态（全局）变量管理接口</summary>
[ApiController]
[Route("api/system/static-var")]
[Authorize]
public class StaticVariableController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;
    public StaticVariableController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("list")]
    public async Task<ApiResult> List([FromQuery] string? groupName)
    {
        var query = _db.StaticVariables.Where(v => v.Deleted == 0);
        if (!string.IsNullOrEmpty(groupName))
            query = query.Where(v => v.GroupName == groupName);
        var list = await query.OrderBy(v => v.GroupName).ThenBy(v => v.Id).ToListAsync();
        return ApiResult.Success(list);
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] StaticVarSaveRequest req)
    {
        if (await _db.StaticVariables.AnyAsync(v => v.VarCode == req.VarCode && v.Deleted == 0))
            return ApiResult.Fail($"变量编码 '{req.VarCode}' 已存在");

        var entity = new StaticVariableEntity
        {
            VarCode      = req.VarCode,
            VarName      = req.VarName,
            DataType     = req.DataType,
            Value        = req.Value ?? req.DefaultValue,
            DefaultValue = req.DefaultValue,
            Description  = req.Description,
            GroupName    = req.GroupName,
            TenantId     = _tenant.TenantId,
            CreatedAt    = DateTime.Now.ToString("o")
        };
        _db.StaticVariables.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = entity.Id });
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] StaticVarSaveRequest req)
    {
        if (req.Id == null) return ApiResult.Fail("id 不能为空");
        var entity = await _db.StaticVariables.FindAsync(req.Id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("变量不存在");

        if (entity.VarCode != req.VarCode &&
            await _db.StaticVariables.AnyAsync(v => v.VarCode == req.VarCode && v.Deleted == 0 && v.Id != req.Id))
            return ApiResult.Fail($"变量编码 '{req.VarCode}' 已存在");

        entity.VarCode      = req.VarCode;
        entity.VarName      = req.VarName;
        entity.DataType     = req.DataType;
        entity.Value        = req.Value;
        entity.DefaultValue = req.DefaultValue;
        entity.Description  = req.Description;
        entity.GroupName    = req.GroupName;
        entity.UpdatedAt    = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>直接修改变量值（流程外手动修改）</summary>
    [HttpPut("setValue/{id}")]
    public async Task<ApiResult> SetValue(long id, [FromBody] SetValueRequest req)
    {
        var entity = await _db.StaticVariables.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("变量不存在");
        entity.Value     = req.Value;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>重置为默认值</summary>
    [HttpPut("reset/{id}")]
    public async Task<ApiResult> Reset(long id)
    {
        var entity = await _db.StaticVariables.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("变量不存在");
        entity.Value     = entity.DefaultValue;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.StaticVariables.FindAsync(id);
        if (entity == null) return ApiResult.Fail("变量不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
