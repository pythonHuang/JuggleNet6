using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using Dm;

namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// 数据库节点执行器：执行 SQL 语句（使用数据源配置的数据库连接）
/// 支持：SQLite / MySQL / PostgreSQL / SQL Server / Oracle / 达梦(DM)
/// 变量使用 Freemarker 风格模板：${varName}
/// </summary>
public class MysqlNodeExecutor : INodeExecutor
{
    private readonly Dictionary<string, DataSourceInfo> _dataSources;

    public MysqlNodeExecutor(Dictionary<string, DataSourceInfo> dataSources)
    {
        _dataSources = dataSources;
    }

    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.MysqlConfig;
        if (cfg == null)
            throw new InvalidOperationException($"数据库节点 [{node.Key}] 未配置 mysqlConfig。");

        // 渲染 SQL（替换模板变量 ${varName}）
        var sql = RenderTemplate(cfg.Sql, context);

        // 获取数据源信息
        if (!_dataSources.TryGetValue(cfg.DataSourceName, out var dsInfo))
            throw new InvalidOperationException($"数据源 [{cfg.DataSourceName}] 未找到，请先在系统设置中配置数据源。");

        // 执行 SQL
        if (cfg.OperationType == "QUERY")
        {
            var results = await ExecuteQueryAsync(dsInfo, sql);
            if (!string.IsNullOrEmpty(cfg.OutputVariable))
            {
                var targetType = cfg.OutputTargetType?.ToUpper() ?? "VARIABLE";
                if (targetType == "OUTPUT")
                    context.SetOutputParameter(cfg.OutputVariable, results);
                else
                    context.SetVariable(cfg.OutputVariable, results);
            }
        }
        else
        {
            var affected = await ExecuteNonQueryAsync(dsInfo, sql);
            if (!string.IsNullOrEmpty(cfg.AffectedRowsVariable))
            {
                var targetType = cfg.AffectedTargetType?.ToUpper() ?? "VARIABLE";
                if (targetType == "OUTPUT")
                    context.SetOutputParameter(cfg.AffectedRowsVariable, affected);
                else
                    context.SetVariable(cfg.AffectedRowsVariable, affected);
            }
        }

        return node.Outgoings.FirstOrDefault();
    }

    private static string RenderTemplate(string sql, FlowContext context)
    {
        return Regex.Replace(sql, @"\$\{([^}]+)\}", m =>
        {
            var varName = m.Groups[1].Value.Trim();
            var val = context.GetVariable(varName);
            return val?.ToString() ?? "";
        });
    }

    private static async Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(DataSourceInfo dsInfo, string sql)
    {
        var results = new List<Dictionary<string, object?>>();
        await using var conn = CreateConnection(dsInfo);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            results.Add(row);
        }
        return results;
    }

    private static async Task<int> ExecuteNonQueryAsync(DataSourceInfo dsInfo, string sql)
    {
        await using var conn = CreateConnection(dsInfo);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>根据数据源类型创建对应的数据库连接</summary>
    public static DbConnection CreateConnection(DataSourceInfo dsInfo)
    {
        return dsInfo.DsType.ToLower() switch
        {
            "sqlite"                    => new SqliteConnection(dsInfo.ConnStr),
            "mysql"                     => new MySqlConnection(dsInfo.ConnStr),
            "postgresql" or "postgres"  => new NpgsqlConnection(dsInfo.ConnStr),
            "sqlserver" or "mssql"      => new SqlConnection(dsInfo.ConnStr),
            "oracle"                    => new OracleConnection(dsInfo.ConnStr),
            "dm"                        => new DmConnection(dsInfo.ConnStr),
            _ => throw new InvalidOperationException($"不支持的数据库类型: {dsInfo.DsType}")
        };
    }

    /// <summary>测试连接是否可用</summary>
    public static async Task<(bool Ok, string Message)> TestConnectionAsync(DataSourceInfo dsInfo)
    {
        try
        {
            await using var conn = CreateConnection(dsInfo);
            await conn.OpenAsync();
            return (true, "连接成功");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

/// <summary>数据源信息（用于节点执行器）</summary>
public class DataSourceInfo
{
    public string DsType { get; set; } = "sqlite";
    public string ConnStr { get; set; } = "";
    public string DsName { get; set; } = "";
}
