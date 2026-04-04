using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/role")]
public class RoleController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public RoleController(JuggleDbContext db) => _db = db;

    /// <summary>角色分页列表</summary>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Roles.Where(r => r.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(r => r.RoleName!.Contains(req.Keyword) || (r.RoleCode != null && r.RoleCode.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(r => r.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        // 查每个角色的菜单权限数
        var roleIds = records.Select(r => r.Id).ToList();
        var menuCounts = await _db.RoleMenus
            .Where(rm => roleIds.Contains(rm.RoleId) && rm.Deleted == 0)
            .GroupBy(rm => rm.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count);

        var result = records.Select(r => new
        {
            r.Id, r.RoleName, r.RoleCode, r.Remark, r.CreatedAt, r.UpdatedAt,
            MenuCount = menuCounts.GetValueOrDefault(r.Id, 0)
        });
        return ApiResult.Success(new { total, records = result });
    }

    /// <summary>所有角色（下拉选择用）</summary>
    [HttpGet("all"), Authorize]
    public async Task<ApiResult> All()
    {
        var list = await _db.Roles
            .Where(r => r.Deleted == 0)
            .OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.RoleName, r.RoleCode })
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

        return ApiResult.Success(new
        {
            role.Id, role.RoleName, role.RoleCode, role.Remark, role.CreatedAt,
            MenuKeys = menuKeys
        });
    }

    /// <summary>新增角色</summary>
    [HttpPost("add"), Authorize]
    public async Task<ApiResult> Add([FromBody] RoleAddRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RoleName))
            return ApiResult.Fail("角色名称不能为空");

        if (!string.IsNullOrWhiteSpace(req.RoleCode))
        {
            var exists = await _db.Roles.AnyAsync(r => r.RoleCode == req.RoleCode && r.Deleted == 0);
            if (exists) return ApiResult.Fail("角色编码已存在");
        }

        var role = new RoleEntity
        {
            RoleName  = req.RoleName,
            RoleCode  = req.RoleCode,
            Remark    = req.Remark,
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
                RoleId    = role.Id,
                MenuKey   = key,
                Deleted   = 0,
                CreatedAt = DateTime.Now.ToString("o")
            }));
            await _db.SaveChangesAsync();
        }

        return ApiResult.Success(new { id = role.Id });
    }

    /// <summary>更新角色</summary>
    [HttpPut("update"), Authorize]
    public async Task<ApiResult> Update([FromBody] RoleUpdateRequest req)
    {
        var role = await _db.Roles.FindAsync(req.Id);
        if (role == null || role.Deleted == 1) return ApiResult.Fail("角色不存在");
        if (role.Id == 1) return ApiResult.Fail("不能修改超级管理员角色");

        role.RoleName  = req.RoleName;
        role.RoleCode  = req.RoleCode;
        role.Remark    = req.Remark;
        role.UpdatedAt = DateTime.Now.ToString("o");

        // 先删除旧权限
        var oldMenus = await _db.RoleMenus
            .Where(rm => rm.RoleId == req.Id && rm.Deleted == 0)
            .ToListAsync();
        foreach (var m in oldMenus) m.Deleted = 1;
        _db.RoleMenus.UpdateRange(oldMenus);

        // 添加新权限
        if (req.MenuKeys.Count > 0)
        {
            _db.RoleMenus.AddRange(req.MenuKeys.Select(key => new RoleMenuEntity
            {
                RoleId    = req.Id,
                MenuKey   = key,
                Deleted   = 0,
                CreatedAt = DateTime.Now.ToString("o")
            }));
        }

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

        // 检查是否有用户使用该角色
        var hasUsers = await _db.Users.AnyAsync(u => u.RoleId == id && u.Deleted == 0);
        if (hasUsers) return ApiResult.Fail("该角色下还有用户，不能删除");

        role.Deleted   = 1;
        role.UpdatedAt = DateTime.Now.ToString("o");

        // 同时删除角色菜单
        var menus = await _db.RoleMenus.Where(rm => rm.RoleId == id && rm.Deleted == 0).ToListAsync();
        foreach (var m in menus) m.Deleted = 1;
        _db.RoleMenus.UpdateRange(menus);

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
