using System.Text.Json;
using JuggleNet6.Backend.Domain.Engine;
using JuggleNet6.Backend.Domain.Engine.NodeExecutors;
using JuggleNet6.Backend.Domain.Entities;
using JuggleNet6.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Services.Flow;

/// <summary>
/// 流程执行核心服务 —— 统一封装数据源加载、静态变量快照/回写、日志持久化、引擎执行。
/// 消除 FlowDefinitionController 与 FlowOpenController 中的代码重复。
/// </summary>
public class FlowExecutionService
{
    private readonly JuggleDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public FlowExecutionService(JuggleDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    // ────────────────────────────────────────────────────────────────
    // 数据源
    // ────────────────────────────────────────────────────────────────

    /// <summary>从数据库加载全部数据源，构建名称 → DataSourceInfo 字典。</summary>
    public async Task<Dictionary<string, DataSourceInfo>> BuildDataSourceInfosAsync()
    {
        var dataSources = await _db.DataSources.Where(d => d.Deleted == 0).ToListAsync();
        var map = new Dictionary<string, DataSourceInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var ds in dataSources)
        {
            if (string.IsNullOrEmpty(ds.DsName)) continue;
            map[ds.DsName] = new DataSourceInfo
            {
                DsType  = (ds.DsType ?? "sqlite").ToLower(),
                ConnStr = BuildConnectionString(ds),
                DsName  = ds.DsName
            };
        }
        return map;
    }

    /// <summary>根据数据源实体构建连接字符串（单条）。</summary>
    public static string BuildConnectionString(DataSourceEntity ds)
    {
        var dsType = (ds.DsType ?? "sqlite").ToLower();
        return dsType switch
        {
            "sqlite"                       => $"Data Source={(string.IsNullOrEmpty(ds.DbName) ? "juggle.db" : ds.DbName)}",
            "mysql"                        => $"Server={ds.Host};Port={ds.Port};Database={ds.DbName};User={ds.Username};Password={ds.Password};CharSet=utf8mb4;",
            "postgresql" or "postgres"     => $"Host={ds.Host};Port={ds.Port};Database={ds.DbName};Username={ds.Username};Password={ds.Password};",
            "sqlserver"  or "mssql"        => $"Server={ds.Host},{ds.Port};Database={ds.DbName};User Id={ds.Username};Password={ds.Password};TrustServerCertificate=True;",
            _                              => $"Data Source={(string.IsNullOrEmpty(ds.DbName) ? "juggle.db" : ds.DbName)}"
        };
    }

    // ────────────────────────────────────────────────────────────────
    // 静态变量
    // ────────────────────────────────────────────────────────────────

    /// <summary>加载全部静态变量，返回 VarCode → Value 快照。</summary>
    public async Task<Dictionary<string, string?>> BuildStaticVariableSnapshotAsync()
    {
        var vars = await _db.StaticVariables.Where(v => v.Deleted == 0).ToListAsync();
        return vars.ToDictionary(v => v.VarCode!, v => v.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>将流程执行中被修改的静态变量回写到数据库。</summary>
    public async Task FlushStaticVariablesAsync(FlowContext context)
    {
        if (context.ModifiedStaticVarCodes.Count == 0) return;
        var codes    = context.ModifiedStaticVarCodes.ToList();
        var entities = await _db.StaticVariables
            .Where(v => v.Deleted == 0 && codes.Contains(v.VarCode!))
            .ToListAsync();
        foreach (var entity in entities)
        {
            if (context.StaticVariables.TryGetValue(entity.VarCode!, out var newVal))
            {
                entity.Value     = newVal;
                entity.UpdatedAt = DateTime.Now.ToString("o");
            }
        }
        await _db.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────
    // 流程日志
    // ────────────────────────────────────────────────────────────────

    /// <summary>将流程执行结果持久化为日志记录（含节点明细）。</summary>
    /// <param name="definition">流程定义实体（用于填写 flowKey/flowName）</param>
    /// <param name="triggerType">触发来源标识：debug / open / version</param>
    /// <param name="version">版本号，为空时从 result.Context 中读取</param>
    public async Task<long> SaveFlowLogAsync(
        FlowDefinitionEntity definition,
        string               triggerType,
        FlowResult           result,
        DateTime             startTime,
        string               inputJson,
        string               version = "")
    {
        var logVersion = string.IsNullOrEmpty(version)
            ? (result.Context?.Version ?? triggerType)
            : version;

        var log = new FlowLogEntity
        {
            FlowKey      = definition.FlowKey,
            FlowName     = definition.FlowName,
            Version      = logVersion,
            TriggerType  = triggerType,
            Status       = result.Success ? "SUCCESS" : "FAILED",
            StartTime    = startTime.ToString("o"),
            EndTime      = DateTime.Now.ToString("o"),
            CostMs       = result.CostMs,
            ErrorMessage = result.ErrorMessage,
            InputJson    = inputJson,
            OutputJson   = JsonSerializer.Serialize(result.OutputData),
            CreatedAt    = startTime.ToString("o")
        };
        _db.FlowLogs.Add(log);
        await _db.SaveChangesAsync();

        // 保存节点明细
        if (result.Context?.NodeLogs.Count > 0)
        {
            var nodeLogs = result.Context.NodeLogs.Select(nl => new FlowNodeLogEntity
            {
                FlowLogId      = log.Id,
                NodeKey        = nl.NodeKey,
                NodeLabel      = nl.NodeLabel,
                NodeType       = nl.NodeType,
                SeqNo          = nl.SeqNo,
                Status         = nl.Status,
                StartTime      = nl.StartTime.ToString("o"),
                EndTime        = nl.EndTime?.ToString("o"),
                CostMs         = nl.CostMs,
                InputSnapshot  = nl.InputSnapshot,
                OutputSnapshot = nl.OutputSnapshot,
                Detail         = nl.Detail,
                ErrorMessage   = nl.ErrorMessage,
                CreatedAt      = nl.StartTime.ToString("o")
            }).ToList();
            _db.FlowNodeLogs.AddRange(nodeLogs);
            await _db.SaveChangesAsync();
        }

        return log.Id;
    }

    // ────────────────────────────────────────────────────────────────
    // 端到端执行
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// 端到端执行流程：加载数据源 + 静态变量 → 运行引擎 → 保存日志 → 回写静态变量。
    /// </summary>
    /// <param name="definition">流程定义（提供 flowKey/flowName/flowContent）</param>
    /// <param name="flowContent">实际运行的节点 JSON（debug 时用 definition.FlowContent，open/version 时用版本快照）</param>
    /// <param name="inputParams">入参字典</param>
    /// <param name="triggerType">debug / open / version</param>
    /// <param name="version">版本号（debug 时传空字符串）</param>
    /// <returns>执行结果（含 LogId、CostMs、Context）</returns>
    public async Task<FlowResult> RunAsync(
        FlowDefinitionEntity            definition,
        string                          flowContent,
        Dictionary<string, object?>     inputParams,
        string                          triggerType,
        string                          version = "")
    {
        var dsInfos     = await BuildDataSourceInfosAsync();
        var staticVars  = await BuildStaticVariableSnapshotAsync();
        var engine      = new FlowEngine(_httpClientFactory, dsInfos, staticVars);

        var inputJson  = JsonSerializer.Serialize(inputParams);
        var startTime  = DateTime.Now;

        var result = await engine.ExecuteAsync(
            flowContent, inputParams, definition.FlowKey!, triggerType);

        // 持久化日志
        var logId = await SaveFlowLogAsync(definition, triggerType, result, startTime, inputJson, version);
        result.LogId = logId;

        // 回写被修改的静态变量
        if (result.Context != null)
            await FlushStaticVariablesAsync(result.Context);

        return result;
    }
}
