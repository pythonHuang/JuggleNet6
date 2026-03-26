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
        return ApiResult.Success();
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
}
