using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/tenant")]
public class TenantController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public TenantController(JuggleDbContext db) => _db = db;

    /// <summary>租户分页列表</summary>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Tenants.Where(t => t.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(t => t.TenantName!.Contains(req.Keyword) || (t.TenantCode != null && t.TenantCode.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(t => t.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(t => new
            {
                t.Id, t.TenantName, t.TenantCode, t.Status, t.Remark, t.CreatedAt, t.UpdatedAt,
                UserCount = _db.Users.Count(u => u.TenantId == t.Id && u.Deleted == 0)
            })
            .ToListAsync();

        return ApiResult.Success(new { total, records });
    }

    /// <summary>所有租户（下拉选择用）</summary>
    [HttpGet("all"), Authorize]
    public async Task<ApiResult> All()
    {
        var list = await _db.Tenants
            .Where(t => t.Deleted == 0 && t.Status == 1)
            .OrderBy(t => t.Id)
            .Select(t => new { t.Id, t.TenantName, t.TenantCode })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>新增租户</summary>
    [HttpPost("add"), Authorize]
    public async Task<ApiResult> Add([FromBody] TenantAddRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TenantName))
            return ApiResult.Fail("租户名称不能为空");

        if (!string.IsNullOrWhiteSpace(req.TenantCode))
        {
            var exists = await _db.Tenants.AnyAsync(t => t.TenantCode == req.TenantCode && t.Deleted == 0);
            if (exists) return ApiResult.Fail("租户编码已存在");
        }

        var tenant = new TenantEntity
        {
            TenantName = req.TenantName,
            TenantCode = req.TenantCode,
            Remark      = req.Remark,
            Status      = 1,
            Deleted     = 0,
            CreatedAt   = DateTime.Now.ToString("o")
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = tenant.Id });
    }

    /// <summary>更新租户</summary>
    [HttpPut("update"), Authorize]
    public async Task<ApiResult> Update([FromBody] TenantUpdateRequest req)
    {
        var tenant = await _db.Tenants.FindAsync(req.Id);
        if (tenant == null || tenant.Deleted == 1) return ApiResult.Fail("租户不存在");
        if (tenant.Id == 1) return ApiResult.Fail("不能修改默认租户");

        tenant.TenantName = req.TenantName;
        tenant.TenantCode = req.TenantCode;
        tenant.Remark      = req.Remark;
        tenant.Status      = req.Status;
        tenant.UpdatedAt   = DateTime.Now.ToString("o");

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>删除租户</summary>
    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ApiResult> Delete(long id)
    {
        if (id == 1) return ApiResult.Fail("不能删除默认租户");
        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant == null || tenant.Deleted == 1) return ApiResult.Fail("租户不存在");

        var hasUsers = await _db.Users.AnyAsync(u => u.TenantId == id && u.Deleted == 0);
        if (hasUsers) return ApiResult.Fail("该租户下还有用户，不能删除");

        tenant.Deleted   = 1;
        tenant.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
