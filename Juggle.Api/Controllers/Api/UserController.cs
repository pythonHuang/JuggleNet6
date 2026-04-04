using Juggle.Domain.Entities;
using Juggle.Infrastructure.Common;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Juggle.Application.Services.Impl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly JwtService      _jwtService;
    private readonly ITenantAccessor _tenant;

    public UserController(JuggleDbContext db, JwtService jwtService, ITenantAccessor tenant)
    {
        _db         = db;
        _jwtService = jwtService;
        _tenant     = tenant;
    }

    [HttpPost("login")]
    public async Task<ApiResult> Login([FromBody] LoginRequest req)
    {
        var pwdMd5 = Md5Helper.Encrypt(req.Password);
        var user   = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName == req.UserName
                                   && u.Password == pwdMd5
                                   && u.Deleted  == 0);

        // 记录登录日志
        var loginIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
        var loginUA = HttpContext.Request.Headers.UserAgent.ToString();

        if (user == null)
        {
            // 记录失败日志
            _db.LoginLogs.Add(new LoginLogEntity
            {
                UserName = req.UserName, LoginType = "login", Result = "fail",
                IpAddress = loginIp, UserAgent = loginUA,
                Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
            });
            await _db.SaveChangesAsync();
            return ApiResult.Fail("用户名或密码错误", 401);
        }

        // 检查租户是否禁用或过期
        if (user.TenantId.HasValue)
        {
            var tenant = await _db.Tenants.FindAsync(user.TenantId.Value);
            if (tenant == null || tenant.Deleted == 1 || tenant.Status == 0)
            {
                _db.LoginLogs.Add(new LoginLogEntity
                {
                    UserId = user.Id, UserName = user.UserName, LoginType = "login", Result = "fail",
                    IpAddress = loginIp, UserAgent = loginUA, TenantId = user.TenantId,
                    Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
                });
                await _db.SaveChangesAsync();
                return ApiResult.Fail("租户已被禁用，无法登录", 403);
            }
            if (tenant.ExpiredAt.HasValue && tenant.ExpiredAt.Value < DateTime.Now)
            {
                _db.LoginLogs.Add(new LoginLogEntity
                {
                    UserId = user.Id, UserName = user.UserName, LoginType = "login", Result = "fail",
                    IpAddress = loginIp, UserAgent = loginUA, TenantId = user.TenantId,
                    Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
                });
                await _db.SaveChangesAsync();
                return ApiResult.Fail("租户已过期，无法登录", 403);
            }
        }

        // 记录成功登录日志
        _db.LoginLogs.Add(new LoginLogEntity
        {
            UserId = user.Id, UserName = user.UserName, LoginType = "login", Result = "success",
            IpAddress = loginIp, UserAgent = loginUA, TenantId = user.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        });
        await _db.SaveChangesAsync();

        var tokenStr = _jwtService.GenerateToken(user);

        // 查询菜单权限（角色权限 ∩ 租户权限）
        List<string> menuKeys = new();
        var isSuperAdmin = user.RoleId == 1;

        if (!isSuperAdmin && user.RoleId.HasValue)
        {
            var roleMenuKeys = await _db.RoleMenus
                .Where(rm => rm.RoleId == user.RoleId && rm.Deleted == 0)
                .Select(rm => rm.MenuKey)
                .ToListAsync();

            var role = await _db.Roles.FindAsync(user.RoleId.Value);
            if (role != null && role.TenantId.HasValue)
            {
                var tenant = await _db.Tenants.FindAsync(role.TenantId.Value);
                if (tenant != null)
                {
                    List<string> tenantMenuKeys;
                    try { tenantMenuKeys = JsonSerializer.Deserialize<List<string>>(tenant.MenuKeys ?? "[]") ?? new(); }
                    catch { tenantMenuKeys = new(); }

                    // 租户有配置菜单权限时取交集，否则不限制
                    if (tenantMenuKeys.Count > 0)
                        menuKeys = roleMenuKeys.Where(k => tenantMenuKeys.Contains(k)).ToList();
                    else
                        menuKeys = roleMenuKeys;
                }
                else
                    menuKeys = roleMenuKeys;
            }
            else
                menuKeys = roleMenuKeys; // 全局角色不受租户限制
        }

        var roleCode = isSuperAdmin ? "admin" : "";
        if (user.RoleId.HasValue && !isSuperAdmin)
        {
            var role = await _db.Roles.FindAsync(user.RoleId.Value);
            if (role != null) roleCode = role.RoleCode ?? "";
        }

        return ApiResult.Success(new
        {
            token = tokenStr,
            userName = user.UserName,
            roleCode,
            menuKeys,
            tenantId = user.TenantId,
            userId = user.Id
        });
    }

    /// <summary>获取当前登录用户信息</summary>
    [HttpGet("info"), Authorize]
    public async Task<ApiResult> Info()
    {
        var user = await _db.Users.FindAsync(_tenant.UserId);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");

        var roleCode = _tenant.IsSuperAdmin ? "admin" : "";
        if (user.RoleId.HasValue && !_tenant.IsSuperAdmin)
        {
            var role = await _db.Roles.FindAsync(user.RoleId.Value);
            if (role != null) roleCode = role.RoleCode ?? "";
        }

        return ApiResult.Success(new
        {
            user.Id, user.UserName,
            RoleId = user.RoleId,
            TenantId = user.TenantId,
            RoleCode = roleCode,
            IsSuperAdmin = _tenant.IsSuperAdmin
        });
    }

    /// <summary>用户分页列表（租户隔离）</summary>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Users.Where(u => u.Deleted == 0);
        if (!_tenant.IsSuperAdmin && _tenant.TenantId.HasValue)
            query = query.Where(u => u.TenantId == _tenant.TenantId);

        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(u => u.UserName!.Contains(req.Keyword));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(u => u.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(u => new
            {
                u.Id, u.UserName, u.RoleId, u.TenantId, u.CreatedAt, u.UpdatedAt,
                RoleName = _db.Roles.Where(r => r.Id == u.RoleId && r.Deleted == 0).Select(r => r.RoleName).FirstOrDefault(),
                TenantName = _db.Tenants.Where(t => t.Id == u.TenantId && t.Deleted == 0).Select(t => t.TenantName).FirstOrDefault()
            })
            .ToListAsync();

        return ApiResult.Success(new { total, records });
    }

    /// <summary>新增用户</summary>
    [HttpPost("add"), Authorize]
    public async Task<ApiResult> Add([FromBody] UserAddRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName))
            return ApiResult.Fail("用户名不能为空");
        if (string.IsNullOrWhiteSpace(req.Password))
            return ApiResult.Fail("密码不能为空");

        var exists = await _db.Users.AnyAsync(u => u.UserName == req.UserName && u.Deleted == 0);
        if (exists) return ApiResult.Fail("用户名已存在");

        // 非超管只能创建本租户用户
        if (!_tenant.IsSuperAdmin)
            req.TenantId = _tenant.TenantId;

        // 验证角色和租户的匹配关系
        if (req.RoleId.HasValue)
        {
            var role = await _db.Roles.FindAsync(req.RoleId.Value);
            if (role == null || role.Deleted == 1)
                return ApiResult.Fail("角色不存在");
            if (role.TenantId != null && role.TenantId != req.TenantId)
                return ApiResult.Fail("角色与用户所属租户不匹配");
        }

        var user = new UserEntity
        {
            UserName = req.UserName, Password = Md5Helper.Encrypt(req.Password),
            RoleId = req.RoleId, TenantId = req.TenantId,
            Deleted = 0, CreatedAt = DateTime.Now.ToString("o")
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = user.Id });
    }

    /// <summary>更新用户</summary>
    [HttpPut("update"), Authorize]
    public async Task<ApiResult> Update([FromBody] UserUpdateRequest req)
    {
        var user = await _db.Users.FindAsync(req.Id);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");

        if (!_tenant.IsSuperAdmin && user.TenantId != _tenant.TenantId)
            return ApiResult.Fail("无权修改其他租户用户", 403);

        // 超管可以改租户，非超管不能
        if (!_tenant.IsSuperAdmin)
            req.TenantId = user.TenantId;

        if (req.RoleId.HasValue)
        {
            var role = await _db.Roles.FindAsync(req.RoleId.Value);
            if (role == null || role.Deleted == 1)
                return ApiResult.Fail("角色不存在");
            var targetTenantId = req.TenantId;
            if (role.TenantId != null && role.TenantId != targetTenantId)
                return ApiResult.Fail("角色与用户所属租户不匹配");
        }

        user.UserName  = req.UserName;
        user.RoleId    = req.RoleId;
        user.TenantId  = req.TenantId;
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>重置密码</summary>
    [HttpPut("resetPwd"), Authorize]
    public async Task<ApiResult> ResetPwd([FromBody] UserResetPwdRequest req)
    {
        var user = await _db.Users.FindAsync(req.Id);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");
        if (!_tenant.IsSuperAdmin && user.TenantId != _tenant.TenantId)
            return ApiResult.Fail("无权操作", 403);
        if (string.IsNullOrWhiteSpace(req.NewPassword))
            return ApiResult.Fail("新密码不能为空");
        user.Password  = Md5Helper.Encrypt(req.NewPassword);
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>修改当前用户密码</summary>
    [HttpPut("changePwd"), Authorize]
    public async Task<ApiResult> ChangePwd([FromBody] UserChangePwdRequest req)
    {
        var userName = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Deleted == 0);
        if (user == null) return ApiResult.Fail("用户不存在");
        if (user.Password != Md5Helper.Encrypt(req.OldPassword))
            return ApiResult.Fail("原密码不正确");
        user.Password  = Md5Helper.Encrypt(req.NewPassword);
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>删除用户</summary>
    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ApiResult> Delete(long id)
    {
        if (id == 1) return ApiResult.Fail("不能删除初始管理员账号");
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");
        if (!_tenant.IsSuperAdmin && user.TenantId != _tenant.TenantId)
            return ApiResult.Fail("无权删除", 403);
        user.Deleted = 1;
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>登录日志分页列表</summary>
    [HttpPost("login-log/page"), Authorize]
    public async Task<ApiResult> LoginLogPage([FromBody] PageRequest req)
    {
        if (!_tenant.IsSuperAdmin)
            return ApiResult.Fail("仅超级管理员可查看", 403);

        var query = _db.LoginLogs.Where(l => l.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(l => (l.UserName != null && l.UserName.Contains(req.Keyword))
                                     || (l.IpAddress != null && l.IpAddress.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(l => l.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(l => new
            {
                l.Id, l.UserId, l.UserName, l.LoginType, l.Result,
                l.IpAddress, l.TenantId, l.CreatedAt
            })
            .ToListAsync();

        return ApiResult.Success(new { total, records });
    }
}
