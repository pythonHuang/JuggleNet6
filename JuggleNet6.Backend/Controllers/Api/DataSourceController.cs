using JuggleNet6.Backend.Domain.Entities;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Models.Request;
using JuggleNet6.Backend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Controllers.Api;

[ApiController]
[Route("api/system/datasource")]
[Authorize]
public class DataSourceController : ControllerBase
{
    private readonly JuggleDbContext _db;

    public DataSourceController(JuggleDbContext db) => _db = db;

    [HttpPost("add")]
    public async Task<ApiResult> Add([FromBody] DataSourceAddRequest req)
    {
        var entity = new DataSourceEntity
        {
            DsName = req.DsName,
            DsType = req.DsType,
            Host = req.Host,
            Port = req.Port,
            DbName = req.DbName,
            Username = req.Username,
            Password = req.Password,
            CreatedAt = DateTime.Now.ToString("o")
        };
        _db.DataSources.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResult.Success(entity.Id);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.DataSources.FindAsync(id);
        if (entity == null) return ApiResult.Fail("数据源不存在");
        entity.Deleted = 1;
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] DataSourceUpdateRequest req)
    {
        var entity = await _db.DataSources.FindAsync(req.Id);
        if (entity == null) return ApiResult.Fail("数据源不存在");
        entity.DsName = req.DsName;
        entity.Host = req.Host;
        entity.Port = req.Port;
        entity.DbName = req.DbName;
        entity.Username = req.Username;
        entity.Password = req.Password;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    [HttpGet("list")]
    public async Task<ApiResult> List()
    {
        var list = await _db.DataSources
            .Where(d => d.Deleted == 0)
            .OrderByDescending(d => d.Id)
            .Select(d => new { dataSourceName = d.DsName, dataSourceType = d.DsType, id = d.Id })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.DataSources.Where(d => d.Deleted == 0);
        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(d => d.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();
        return ApiResult.Success(new PageResult<DataSourceEntity>
        {
            Total = total, PageNum = req.PageNum, PageSize = req.PageSize, Records = records
        });
    }
}
