using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/flow/info")]
[Authorize]
public class FlowInfoController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public FlowInfoController(JuggleDbContext db) => _db = db;

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.FlowInfos.Where(f => f.Deleted == 0);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(f => f.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<FlowInfoEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpGet("info/{id}")]
    public async Task<ApiResult> Info(long id)
    {
        var entity = await _db.FlowInfos.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("流程不存在");
        return ApiResult.Success(entity);
    }
}
