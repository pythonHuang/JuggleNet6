using Juggle.Domain.Entities;
using Juggle.Infrastructure.Common;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Application.Services.Impl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly JwtService      _jwtService;

    public UserController(JuggleDbContext db, JwtService jwtService)
    {
        _db         = db;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ApiResult> Login([FromBody] LoginRequest req)
    {
        var pwdMd5 = Md5Helper.Encrypt(req.Password);
        var user   = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName == req.UserName
                                   && u.Password == pwdMd5
                                   && u.Deleted  == 0);
        if (user == null)
            return ApiResult.Fail("用户名或密码错误", 401);

        var tokenStr = _jwtService.GenerateToken(user);
        return ApiResult.Success(new { token = tokenStr, userName = user.UserName });
    }

    /// <summary>用户分页列表（需登录）</summary>
    [HttpPost("page"), Authorize]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Users.Where(u => u.Deleted == 0);
        if (!string.IsNullOrEmpty(req.Keyword))
            query = query.Where(u => u.UserName!.Contains(req.Keyword));
        var total = await query.CountAsync();
        var records = await query.OrderByDescending(u => u.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(u => new { u.Id, u.UserName, u.CreatedAt, u.UpdatedAt })
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

        var user = new UserEntity
        {
            UserName  = req.UserName,
            Password  = Md5Helper.Encrypt(req.Password),
            Deleted   = 0,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = user.Id });
    }

    /// <summary>重置密码</summary>
    [HttpPut("resetPwd"), Authorize]
    public async Task<ApiResult> ResetPwd([FromBody] UserResetPwdRequest req)
    {
        var user = await _db.Users.FindAsync(req.Id);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");
        if (string.IsNullOrWhiteSpace(req.NewPassword))
            return ApiResult.Fail("新密码不能为空");
        user.Password  = Md5Helper.Encrypt(req.NewPassword);
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    /// <summary>修改当前用户密码（需验证旧密码）</summary>
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

    /// <summary>删除用户（不能删除 id=1 的初始管理员）</summary>
    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ApiResult> Delete(long id)
    {
        if (id == 1) return ApiResult.Fail("不能删除初始管理员账号");
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Deleted == 1) return ApiResult.Fail("用户不存在");
        user.Deleted   = 1;
        user.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
