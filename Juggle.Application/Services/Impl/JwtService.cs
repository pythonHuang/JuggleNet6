using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Juggle.Application.Services.Impl;

/// <summary>JWT 签发服务，将 Token 生成逻辑从 UserController 中解耦。</summary>
public class JwtService
{
    private readonly IConfiguration _config;
    private readonly JuggleDbContext _db;

    public JwtService(IConfiguration config, JuggleDbContext db)
    {
        _config = config;
        _db = db;
    }

    /// <summary>为指定用户签发 JWT Token（有效期 24 小时）。</summary>
    public async Task<string> GenerateTokenAsync(UserEntity user)
    {
        return GenerateToken(user);
    }

    /// <summary>同步版（供 Controller 直接调用）。</summary>
    public string GenerateToken(UserEntity user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "JuggleNet6SecretKey2026!";
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.UserName ?? "")
        };

        // 角色信息 - 用 RoleId 直接存 Claim，不查库（登录接口自己查）
        if (user.RoleId.HasValue)
        {
            claims.Add(new Claim("RoleId", user.RoleId.Value.ToString()));
        }

        // 租户信息
        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer:             "JuggleNet6",
            audience:           "JuggleNet6",
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
