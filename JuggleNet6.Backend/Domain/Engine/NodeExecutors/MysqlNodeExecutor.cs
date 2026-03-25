using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace JuggleNet6.Backend.Domain.Engine.NodeExecutors;

/// <summary>
/// MYSQL 节点执行器：执行 SQL 语句（使用数据源配置的数据库连接）
/// 当前支持 SQLite 和 MySQL（需要 MySql.Data 包，未引入时降级为 SQLite）
/// 变量使用 Freemarker 风格模板：${varName}
/// </summary>
public class MysqlNodeExecutor : INodeExecutor
{
    private readonly Dictionary<string, string> _connectionStrings;

    public MysqlNodeExecutor(Dictionary<string, string> connectionStrings)
    {
        _connectionStrings = connectionStrings;
    }

    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.MysqlConfig;
        if (cfg == null)
            throw new InvalidOperationException($"MYSQL node [{node.Key}] has no mysql config.");

        // 渲染 SQL（替换模板变量 ${varName}）
        var sql = RenderTemplate(cfg.Sql, context);

        // 获取连接字符串
        if (!_connectionStrings.TryGetValue(cfg.DataSourceName, out var connStr))
            throw new InvalidOperationException($"数据源 [{cfg.DataSourceName}] 未找到，请先在系统设置中配置数据源。");

        // 执行 SQL
        if (cfg.OperationType == "QUERY")
        {
            var results = await ExecuteQueryAsync(connStr, sql, cfg.DataSourceType);
            // 将查询结果写入目标变量
            if (!string.IsNullOrEmpty(cfg.OutputVariable))
            {
                context.SetVariable(cfg.OutputVariable, results);
            }
        }
        else
        {
            // INSERT / UPDATE / DELETE（更改操作，不关心结果）
            await ExecuteNonQueryAsync(connStr, sql, cfg.DataSourceType);
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

    private static async Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(
        string connStr, string sql, string? dsType)
    {
        var results = new List<Dictionary<string, object?>>();

        // 默认使用 SQLite（生产场景可扩展 MySQL 连接）
        using var conn = new SqliteConnection(connStr);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            results.Add(row);
        }

        return results;
    }

    private static async Task ExecuteNonQueryAsync(string connStr, string sql, string? dsType)
    {
        using var conn = new SqliteConnection(connStr);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}
