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
[Route("api/audit-log")]
public class AuditLogController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    public AuditLogController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>审计日志分页列表（超级管理员）</summary>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] AuditLogPageRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("仅超级管理员可查看", 403);

        var query = _db.AuditLogs.Where(a => a.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Module))
            query = query.Where(a => a.Module == req.Module);
        if (!string.IsNullOrEmpty(req.ActionType))
            query = query.Where(a => a.ActionType == req.ActionType);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(a => a.ChangeContent.Contains(req.Keyword)
                                     || (a.OperatorName != null && a.OperatorName.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(a => a.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new
            {
                a.Id, a.Module, a.ActionType, a.TargetId, a.ChangeContent,
                a.OperatorName, a.OperatorId, a.OperatorTenantId, a.CreatedAt
            })
            .ToListAsync();

        return ApiResult.Success(new { total, records });
    }
}
