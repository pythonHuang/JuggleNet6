using JuggleNet6.Backend.Infrastructure.Common;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JuggleNet6.Backend.Controllers.Api;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly IConfiguration _config;

    public UserController(JuggleDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ApiResult> Login([FromBody] LoginRequest req)
    {
        var pwdMd5 = Md5Helper.Encrypt(req.Password);
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName == req.UserName && u.Password == pwdMd5 && u.Deleted == 0);

        if (user == null)
            return ApiResult.Fail("用户名或密码错误", 401);

        var jwtKey = _config["Jwt:Key"] ?? "JuggleNet6SecretKey2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? "")
        };
        var token = new JwtSecurityToken(
            issuer: "JuggleNet6",
            audience: "JuggleNet6",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return ApiResult.Success(new { token = tokenStr, userName = user.UserName });
    }
}
