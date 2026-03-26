using JuggleNet6.Backend.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JuggleNet6.Backend.Services.Impl;

/// <summary>JWT 签发服务，将 Token 生成逻辑从 UserController 中解耦。</summary>
public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    /// <summary>为指定用户签发 JWT Token（有效期 24 小时）。</summary>
    public string GenerateToken(UserEntity user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "JuggleNet6SecretKey2026!";
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.UserName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer:             "JuggleNet6",
            audience:           "JuggleNet6",
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
