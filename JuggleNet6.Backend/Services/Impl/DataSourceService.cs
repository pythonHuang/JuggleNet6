using JuggleNet6.Backend.Domain.Engine.NodeExecutors;
using JuggleNet6.Backend.Domain.Entities;
using JuggleNet6.Backend.Infrastructure.Persistence;
using JuggleNet6.Backend.Services.Flow;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Services.Impl;

/// <summary>
/// 数据源服务 —— 统一管理连接字符串构建和连接测试逻辑，
/// 消除 DataSourceController.Test、FlowExecutionService.BuildDataSourceInfos 中的重复实现。
/// </summary>
public class DataSourceService
{
    private readonly JuggleDbContext _db;

    public DataSourceService(JuggleDbContext db) => _db = db;

    /// <summary>构建连接字符串（委托给 FlowExecutionService 的静态方法，保持单一来源）。</summary>
    public static string BuildConnectionString(DataSourceEntity ds)
        => FlowExecutionService.BuildConnectionString(ds);

    /// <summary>通过 ID 查找数据源并测试连接。</summary>
    public async Task<(bool Ok, string Message)> TestConnectionAsync(long id)
    {
        var ds = await _db.DataSources.FindAsync(id);
        if (ds == null) return (false, "数据源不存在");

        var connStr = BuildConnectionString(ds);
        var dsInfo  = new DataSourceInfo
        {
            DsType  = (ds.DsType ?? "sqlite").ToLower(),
            ConnStr = connStr,
            DsName  = ds.DsName ?? ""
        };
        return await MysqlNodeExecutor.TestConnectionAsync(dsInfo);
    }
}
