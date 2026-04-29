using Juggle.Domain.Entities;
using Juggle.Infrastructure.Common;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Juggle.Api.Controllers.Api;

/// <summary>
/// 租户管理控制器
/// 提供租户的增删改查、审计日志查看等功能
/// 仅超级管理员可访问
/// </summary>
[ApiController]
[Route("api/tenant")]
public class TenantController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="tenant">多租户访问器</param>
    public TenantController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>
    /// 租户分页列表
    /// 仅超级管理员可访问
    /// </summary>
    /// <param name="req">分页请求参数</param>
    /// <returns>租户列表（包含菜单权限和用户数量）</returns>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("无权访问", 403);

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
                t.ExpiredAt,
                MenuKeysJson = t.MenuKeys,
                UserCount = _db.Users.Count(u => u.TenantId == t.Id && u.Deleted == 0)
            })
            .ToListAsync();

        // 解析 MenuKeys JSON
        var result = records.Select(t =>
        {
            List<string>? keys = null;
            try { keys = JsonSerializer.Deserialize<List<string>>(t.MenuKeysJson ?? "[]"); } catch { keys = new(); }
            return new
            {
                t.Id, t.TenantName, t.TenantCode, t.Status, t.Remark, t.CreatedAt, t.UpdatedAt,
                t.ExpiredAt,
                MenuKeys = keys ?? new(),
                MenuCount = keys?.Count ?? 0,
                t.UserCount
            };
        }).ToList();

        return ApiResult.Success(new { total, records = result });
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

    /// <summary>所有启用租户（下拉选择用，兼容前端 /active 路由）</summary>
    [HttpGet("active"), Authorize]
    public async Task<ApiResult> Active()
    {
        var list = await _db.Tenants
            .Where(t => t.Deleted == 0 && t.Status == 1)
            .OrderBy(t => t.Id)
            .Select(t => new { t.Id, t.TenantName, t.TenantCode })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>租户详情（含菜单权限和关联用户）</summary>
    [HttpGet("detail/{id}"), Authorize]
    public async Task<ApiResult> Detail(long id)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("无权访问", 403);

        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant == null || tenant.Deleted == 1) return ApiResult.Fail("租户不存在");

        List<string> menuKeys;
        try { menuKeys = JsonSerializer.Deserialize<List<string>>(tenant.MenuKeys ?? "[]") ?? new(); }
        catch { menuKeys = new(); }

        var users = await _db.Users
            .Where(u => u.TenantId == id && u.Deleted == 0)
            .Select(u => new { u.Id, u.UserName, u.RoleId })
            .ToListAsync();

        return ApiResult.Success(new
        {
            tenant.Id, tenant.TenantName, tenant.TenantCode, tenant.Status,
            tenant.Remark, tenant.ExpiredAt, tenant.CreatedAt,
            MenuKeys = menuKeys,
            UserIds = users.Select(u => u.Id).ToList(),
            Users = users
        });
    }

    /// <summary>新增租户（仅超级管理员）</summary>
    [HttpPost("add"), Authorize]
    public async Task<ApiResult> Add([FromBody] TenantAddRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("仅超级管理员可操作", 403);
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
            ExpiredAt   = req.ExpiredAt,
            MenuKeys    = JsonSerializer.Serialize(req.MenuKeys),
            Deleted     = 0,
            CreatedAt   = DateTime.Now.ToString("o")
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        // 关联用户到该租户
        if (req.UserIds.Count > 0)
        {
            var users = await _db.Users.Where(u => req.UserIds.Contains(u.Id) && u.Deleted == 0).ToListAsync();
            foreach (var u in users) u.TenantId = tenant.Id;
            _db.Users.UpdateRange(users);
            await _db.SaveChangesAsync();
        }

        await AddAuditLog("tenant", "add", tenant.Id, $"新增租户：{req.TenantName}");
        return ApiResult.Success(new { id = tenant.Id });
    }

    /// <summary>更新租户（仅超级管理员）</summary>
    [HttpPut("update"), Authorize]
    public async Task<ApiResult> Update([FromBody] TenantUpdateRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("仅超级管理员可操作", 403);

        var tenant = await _db.Tenants.FindAsync(req.Id);
        if (tenant == null || tenant.Deleted == 1) return ApiResult.Fail("租户不存在");
        if (tenant.Id == 1) return ApiResult.Fail("不能修改默认租户");

        tenant.TenantName = req.TenantName;
        tenant.TenantCode = req.TenantCode;
        tenant.Remark      = req.Remark;
        tenant.Status      = req.Status;
        tenant.ExpiredAt   = req.ExpiredAt;
        tenant.MenuKeys    = JsonSerializer.Serialize(req.MenuKeys);
        tenant.UpdatedAt   = DateTime.Now.ToString("o");

        // 更新关联用户
        var currentUsers = await _db.Users.Where(u => u.TenantId == req.Id && u.Deleted == 0).Select(u => u.Id).ToListAsync();
        var removeUserIds = currentUsers.Except(req.UserIds).ToList();
        var addUserIds = req.UserIds.Except(currentUsers).ToList();

        if (removeUserIds.Count > 0)
        {
            var removeUsers = await _db.Users.Where(u => removeUserIds.Contains(u.Id) && u.Deleted == 0).ToListAsync();
            foreach (var u in removeUsers) u.TenantId = null;
            _db.Users.UpdateRange(removeUsers);
        }
        if (addUserIds.Count > 0)
        {
            var addUsers = await _db.Users.Where(u => addUserIds.Contains(u.Id) && u.Deleted == 0).ToListAsync();
            foreach (var u in addUsers) u.TenantId = req.Id;
            _db.Users.UpdateRange(addUsers);
        }

        await _db.SaveChangesAsync();
        await AddAuditLog("tenant", "update", req.Id, $"更新租户：{req.TenantName}");
        return ApiResult.Success();
    }

    /// <summary>删除租户（仅超级管理员）</summary>
    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ApiResult> Delete(long id)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("仅超级管理员可操作", 403);
        if (id == 1) return ApiResult.Fail("不能删除默认租户");

        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant == null || tenant.Deleted == 1) return ApiResult.Fail("租户不存在");

        var hasUsers = await _db.Users.AnyAsync(u => u.TenantId == id && u.Deleted == 0);
        if (hasUsers) return ApiResult.Fail("该租户下还有用户，请先移除关联用户");

        tenant.Deleted = 1;
        tenant.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();

        await AddAuditLog("tenant", "delete", id, $"删除租户：{tenant.TenantName}");
        return ApiResult.Success();
    }

    /// <summary>审计日志分页列表</summary>
    [HttpPost("audit/page"), Authorize]
    public async Task<ApiResult> AuditPage([FromBody] AuditLogPageRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("无权访问", 403);

        var query = _db.AuditLogs.Where(a => a.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Module))
            query = query.Where(a => a.Module == req.Module);
        if (!string.IsNullOrEmpty(req.ActionType))
            query = query.Where(a => a.ActionType == req.ActionType);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(a => a.ChangeContent.Contains(req.Keyword) || (a.OperatorName != null && a.OperatorName.Contains(req.Keyword)));

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

    private async Task AddAuditLog(string module, string actionType, long targetId, string changeContent)
    {
        _db.AuditLogs.Add(new AuditLogEntity
        {
            Module = module, ActionType = actionType, TargetId = targetId,
            ChangeContent = changeContent, OperatorName = _tenant.UserName,
            OperatorId = _tenant.UserId, OperatorTenantId = _tenant.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        });
        await _db.SaveChangesAsync();
    }
}
