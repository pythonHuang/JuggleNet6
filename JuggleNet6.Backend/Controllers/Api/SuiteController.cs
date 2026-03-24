using JuggleNet6.Backend.Domain.Entities;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Api;

[ApiController]
[Route("api/suite")]
[Authorize]
public class SuiteController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public SuiteController(JuggleDbContext db) => _db = db;

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] SuiteAddRequest req)
    {
        var code = $"suite_{Guid.NewGuid():N}";
        var entity = new SuiteEntity
        {
            SuiteCode = code,
            SuiteName = req.SuiteName,
            SuiteDesc = req.SuiteDesc,
            SuiteImage = req.SuiteImage,
            SuiteVersion = req.SuiteVersion,
            SuiteFlag = 0,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.Suites.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(entity.Id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Suites.FindAsync(id);
        if (entity == null) return ApiResult.Fail("套件不存在");
        entity.Deleted = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] SuiteUpdateRequest req)
    {
        var entity = await _db.Suites.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("套件不存在");
        entity.SuiteName = req.SuiteName;
        entity.SuiteDesc = req.SuiteDesc;
        entity.SuiteImage = req.SuiteImage;
        entity.SuiteVersion = req.SuiteVersion;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("info/{id}")]
    public async Task<ApiResult> Info(long id)
    {
        var entity = await _db.Suites.FindAsync(id);
        if (entity == null || entity.Deleted == 1) return ApiResult.Fail("套件不存在");
        return ApiResult.Success(entity);
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] SuitePageRequest req)
    {
        var query = _db.Suites.Where(s => s.Deleted == 0);
        if (!string.IsNullOrEmpty(req.SuiteName))
            query = query.Where(s => s.SuiteName!.Contains(req.SuiteName));
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(s => s.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<SuiteEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }

    [HttpGet("list")]
    public async Task<ApiResult> List()
    {
        var list = await _db.Suites.Where(s => s.Deleted == 0).OrderBy(s => s.SuiteName).ToListAsync();
        return ApiResult.Success(list);
    }
}
