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
[Route("api/object")]
[Authorize]
public class ObjectController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    public ObjectController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] ObjectAddRequest req)
    {
        var code = $"obj_{Guid.NewGuid():N}";
        var entity = new ObjectEntity
        {
            ObjectCode = code,
            ObjectName = req.ObjectName,
            ObjectDesc = req.ObjectDesc,
            TenantId   = _tenant.TenantId,
            CreatedAt  = DateTime.Now.ToString("o")
        };
        _db.Objects.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(entity.Id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Objects.FindAsync(id);
        if (entity == null) return ApiResult.Fail("对象不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] ObjectUpdateRequest req)
    {
        var entity = await _db.Objects.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("对象不存在");
        entity.ObjectName = req.ObjectName;
        entity.ObjectDesc = req.ObjectDesc;
        entity.UpdatedAt  = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Objects.Where(o => o.Deleted == 0);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(o => o.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<ObjectEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpGet("list")]
    public async Task<ApiResult> List()
    {
        var list = await _db.Objects.Where(o => o.Deleted == 0).OrderBy(o => o.ObjectName).ToListAsync();
        return ApiResult.Success(list);
    }
}
