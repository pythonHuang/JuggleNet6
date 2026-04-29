using Juggle.Domain.Entities;
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
/// 角色管理控制器
/// 提供角色的增删改查、权限分配等功能
/// </summary>
[ApiController]
[Route("api/role")]
public class RoleController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="tenant">多租户访问器</param>
    public RoleController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>
    /// 角色分页列表
    /// 普通用户只能看到本租户角色和全局角色
    /// </summary>
    /// <param name="req">分页请求参数</param>
    /// <returns>角色列表（包含租户名称和菜单权限数量）</returns>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        // 所有用户只看本租户角色和全局角色(TenantId=null)
        var query = _db.Roles.Where(r => r.Deleted == 0)
            .Where(r => r.TenantId == null || r.TenantId == _tenant.TenantId);

        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(r => r.RoleName!.Contains(req.Keyword) || (r.RoleCode != null && r.RoleCode.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(r => r.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new
            {
                r.Id, r.RoleName, r.RoleCode, r.Remark, r.CreatedAt, r.UpdatedAt,
                r.TenantId,
                TenantName = _db.Tenants.Where(t => t.Id == r.TenantId && t.Deleted == 0).Select(t => t.TenantName).FirstOrDefault()!,
                MenuCount = _db.RoleMenus.Count(rm => rm.RoleId == r.Id && rm.Deleted == 0)
            })
            .ToListAsync();

        return ApiResult.Success(new { total, records });
    }

    /// <summary>角色下拉列表（根据权限过滤）</summary>
    [HttpGet("all"), Authorize]
    public async Task<ApiResult> All()
    {
        var query = _db.Roles.Where(r => r.Deleted == 0);
        if (!_tenant.IsSuperAdmin)
            query = query.Where(r => r.TenantId == null || r.TenantId == _tenant.TenantId);

        var list = await query.OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.RoleName, r.RoleCode, r.TenantId })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>根据租户ID获取角色列表（用户编辑时使用）</summary>
    [HttpGet("byTenant/{tenantId}"), Authorize]
    public async Task<ApiResult> ByTenant(long tenantId)
    {
        // tenantId=0 时返回全局角色（无租户绑定）
        if (tenantId == 0)
        {
            var globalList = await _db.Roles
                .Where(r => r.Deleted == 0 && r.TenantId == null)
                .OrderBy(r => r.Id)
                .Select(r => new { r.Id, r.RoleName, r.RoleCode, r.TenantId })
                .ToListAsync();
            return ApiResult.Success(globalList);
        }

        // 超管可获取指定租户下所有角色
        // 非超管只能获取本租户的角色
        if (!_tenant.IsSuperAdmin && tenantId != _tenant.TenantId)
            return ApiResult.Fail("无权访问该租户角色", 403);

        var list = await _db.Roles
            .Where(r => r.Deleted == 0 && (r.TenantId == tenantId || r.TenantId == null))
            .OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.RoleName, r.RoleCode, r.TenantId })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>角色详情（含菜单权限）</summary>
    [HttpGet("detail/{id}"), Authorize]
    public async Task<ApiResult> Detail(long id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null || role.Deleted == 1) return ApiResult.Fail("角色不存在");

        var menuKeys = await _db.RoleMenus
            .Where(rm => rm.RoleId == id && rm.Deleted == 0)
            .Select(rm => rm.MenuKey)
            .ToListAsync();

        // 获取角色所属租户的最大菜单权限
        List<string> tenantMenuKeys = new();
        if (role.TenantId.HasValue)
        {
            var tenant = await _db.Tenants.FindAsync(role.TenantId.Value);
            if (tenant != null)
            {
                try { tenantMenuKeys = JsonSerializer.Deserialize<List<string>>(tenant.MenuKeys ?? "[]") ?? new(); }
                catch { tenantMenuKeys = new(); }
            }
        }

        return ApiResult.Success(new
        {
            role.Id, role.RoleName, role.RoleCode, role.Remark, role.CreatedAt,
            role.TenantId,
            MenuKeys = menuKeys,
            TenantMenuKeys = tenantMenuKeys // 角色可分配的最大权限边界
        });
    }

    /// <summary>新增角色</summary>
    [HttpPost("add"), Authorize]
    public async Task<ApiResult> Add([FromBody] RoleAddRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RoleName))
            return ApiResult.Fail("角色名称不能为空");

        // 非超管不能设置租户
        if (!_tenant.IsSuperAdmin && req.TenantId != null)
            return ApiResult.Fail("无权指定租户", 403);

        // 非超管只能创建本租户角色或全局角色
        if (!_tenant.IsSuperAdmin && req.TenantId != null && req.TenantId != _tenant.TenantId)
            return ApiResult.Fail("只能创建本租户的角色", 403);

        // 租户编码唯一性
        if (!string.IsNullOrWhiteSpace(req.RoleCode))
        {
            var exists = await _db.Roles.AnyAsync(r => r.RoleCode == req.RoleCode && r.Deleted == 0);
            if (exists) return ApiResult.Fail("角色编码已存在");
        }

        // 如果角色绑定租户，校验菜单权限不超出租户权限
        if (req.TenantId.HasValue)
        {
            var tenant = await _db.Tenants.FindAsync(req.TenantId.Value);
            if (tenant != null)
            {
                List<string> tenantMenuKeys;
                try { tenantMenuKeys = JsonSerializer.Deserialize<List<string>>(tenant.MenuKeys ?? "[]") ?? new(); }
                catch { tenantMenuKeys = new(); }

                if (tenantMenuKeys.Count > 0)
                {
                    var overKeys = req.MenuKeys.Where(k => !tenantMenuKeys.Contains(k)).ToList();
                    if (overKeys.Count > 0)
                        return ApiResult.Fail($"角色权限超出租户权限范围：{string.Join(", ", overKeys)}");
                }
            }
        }

        var role = new RoleEntity
        {
            RoleName  = req.RoleName,
            RoleCode  = req.RoleCode,
            Remark    = req.Remark,
            TenantId  = req.TenantId,
            Deleted   = 0,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        // 保存菜单权限
        if (req.MenuKeys.Count > 0)
        {
            _db.RoleMenus.AddRange(req.MenuKeys.Select(key => new RoleMenuEntity
            {
                RoleId = role.Id, MenuKey = key, Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
            }));
            await _db.SaveChangesAsync();
        }

        _db.AuditLogs.Add(new AuditLogEntity
        {
            Module = "role", ActionType = "add", TargetId = role.Id,
            ChangeContent = $"新增角色：{req.RoleName}",
            OperatorName = _tenant.UserName, OperatorId = _tenant.UserId,
            OperatorTenantId = _tenant.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        });
        await _db.SaveChangesAsync();

        return ApiResult.Success(new { id = role.Id });
    }

    /// <summary>更新角色</summary>
    [HttpPut("update"), Authorize]
    public async Task<ApiResult> Update([FromBody] RoleUpdateRequest req)
    {
        var role = await _db.Roles.FindAsync(req.Id);
        if (role == null || role.Deleted == 1) return ApiResult.Fail("角色不存在");
        if (role.Id == 1) return ApiResult.Fail("不能修改超级管理员角色");

        // 非超管不能修改租户
        if (!_tenant.IsSuperAdmin && req.TenantId != role.TenantId)
            return ApiResult.Fail("无权修改角色所属租户", 403);

        // 非超管不能把角色移到其他租户
        if (!_tenant.IsSuperAdmin && req.TenantId != null && req.TenantId != _tenant.TenantId)
            return ApiResult.Fail("无权将角色移至其他租户", 403);

        // 如果修改了租户，重新校验权限
        if (req.TenantId.HasValue)
        {
            var tenant = await _db.Tenants.FindAsync(req.TenantId.Value);
            if (tenant != null)
            {
                List<string> tenantMenuKeys;
                try { tenantMenuKeys = JsonSerializer.Deserialize<List<string>>(tenant.MenuKeys ?? "[]") ?? new(); }
                catch { tenantMenuKeys = new(); }

                if (tenantMenuKeys.Count > 0)
                {
                    var overKeys = req.MenuKeys.Where(k => !tenantMenuKeys.Contains(k)).ToList();
                    if (overKeys.Count > 0)
                        return ApiResult.Fail($"角色权限超出租户权限范围：{string.Join(", ", overKeys)}");
                }
            }
        }

        role.RoleName  = req.RoleName;
        role.RoleCode  = req.RoleCode;
        role.Remark    = req.Remark;
        role.TenantId  = req.TenantId;
        role.UpdatedAt = DateTime.Now.ToString("o");

        // 先删除旧权限
        var oldMenus = await _db.RoleMenus.Where(rm => rm.RoleId == req.Id && rm.Deleted == 0).ToListAsync();
        foreach (var m in oldMenus) m.Deleted = 1;
        _db.RoleMenus.UpdateRange(oldMenus);

        // 添加新权限
        if (req.MenuKeys.Count > 0)
        {
            _db.RoleMenus.AddRange(req.MenuKeys.Select(key => new RoleMenuEntity
            {
                RoleId = req.Id, MenuKey = key, Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
            }));
        }

        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLogEntity
        {
            Module = "role", ActionType = "update", TargetId = req.Id,
            ChangeContent = $"更新角色：{req.RoleName}",
            OperatorName = _tenant.UserName, OperatorId = _tenant.UserId,
            OperatorTenantId = _tenant.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        });
        await _db.SaveChangesAsync();

        return ApiResult.Success();
    }

    /// <summary>删除角色</summary>
    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ApiResult> Delete(long id)
    {
        if (id == 1) return ApiResult.Fail("不能删除超级管理员角色");

        var role = await _db.Roles.FindAsync(id);
        if (role == null || role.Deleted == 1) return ApiResult.Fail("角色不存在");

        // 非超管只能删除本租户角色
        if (!_tenant.IsSuperAdmin && role.TenantId != _tenant.TenantId && role.TenantId != null)
            return ApiResult.Fail("无权删除该角色", 403);

        var hasUsers = await _db.Users.AnyAsync(u => u.RoleId == id && u.Deleted == 0);
        if (hasUsers) return ApiResult.Fail("该角色下还有用户，不能删除");

        role.Deleted = 1;
        role.UpdatedAt = DateTime.Now.ToString("o");

        var menus = await _db.RoleMenus.Where(rm => rm.RoleId == id && rm.Deleted == 0).ToListAsync();
        foreach (var m in menus) m.Deleted = 1;
        _db.RoleMenus.UpdateRange(menus);

        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLogEntity
        {
            Module = "role", ActionType = "delete", TargetId = id,
            ChangeContent = $"删除角色：{role.RoleName}",
            OperatorName = _tenant.UserName, OperatorId = _tenant.UserId,
            OperatorTenantId = _tenant.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        });
        await _db.SaveChangesAsync();

        return ApiResult.Success();
    }
}
