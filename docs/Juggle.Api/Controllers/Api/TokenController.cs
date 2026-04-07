using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/system/token")]
[Authorize]
public class TokenController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public TokenController(JuggleDbContext db) => _db = db;

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] TokenAddRequest req)
    {
        var tokenValue = Guid.NewGuid().ToString("N");
        var entity = new TokenEntity
        {
            TokenValue = tokenValue,
            TokenName  = req.TokenName,
            ExpiredAt  = req.ExpiredAt,
            Status     = 1,
            CreatedAt  = DateTime.Now.ToString("o")
        };
        _db.Tokens.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = entity.Id, tokenValue });
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Tokens.FindAsync(id);
        if (entity == null) return ApiResult.Fail("Token不存在");
        entity.Deleted   = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success("删除成功");
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Tokens.Where(t => t.Deleted == 0);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(t => t.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<TokenEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    // ===== 权限管理 =====

    /// <summary>获取 Token 的权限列表</summary>
    [HttpGet("permissions/{tokenId}")]
    public async Task<ApiResult> GetPermissions(long tokenId)
    {
        var list = await _db.TokenPermissions
            .Where(p => p.TokenId == tokenId && p.Deleted == 0)
            .ToListAsync();
        return ApiResult.Success(list);
    }

    /// <summary>保存 Token 的权限列表（全量覆盖）</summary>
    [HttpPost("permissions/{tokenId}")]
    public async Task<ApiResult> SavePermissions(long tokenId, [FromBody] List<TokenPermissionSaveRequest> permissions)
    {
        var token = await _db.Tokens.FindAsync(tokenId);
        if (token == null || token.Deleted == 1) return ApiResult.Fail("Token不存在");

        // 删除旧权限
        var oldPerms = await _db.TokenPermissions
            .Where(p => p.TokenId == tokenId && p.Deleted == 0)
            .ToListAsync();
        foreach (var p in oldPerms) p.Deleted = 1;
        _db.TokenPermissions.UpdateRange(oldPerms);

        // 添加新权限
        foreach (var perm in permissions)
        {
            _db.TokenPermissions.Add(new TokenPermissionEntity
            {
                TokenId        = tokenId,
                PermissionType = perm.PermissionType,
                ResourceKey    = perm.ResourceKey,
                ResourceName   = perm.ResourceName,
                CreatedAt      = DateTime.Now.ToString("o")
            });
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }
}
