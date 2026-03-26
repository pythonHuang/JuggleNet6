using JuggleNet6.Backend.Infrastructure.Common;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using JuggleNet6.Backend.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Api;

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
}
