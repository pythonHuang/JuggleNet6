using Juggle.Domain.Engine;
using Juggle.Infrastructure.Persistence;
using Juggle.Application.Services.Flow;
using Juggle.Application.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// ==================== 服务注册 ====================

// JSON 配置（camelCase 输出，中文不转义）
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy        = JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Encoder                     = JavaScriptEncoder.Create(UnicodeRanges.All);
});

// ==================== 数据库配置（支持多种数据库） ====================
// 通过环境变量切换：DB_TYPE=sqlite|sqlserver|mysql|postgresql
// 连接字符串通过：DB_CONNECTION_STRING 或各数据库专属变量设置
var dbType = (Environment.GetEnvironmentVariable("DB_TYPE") ?? "sqlite").ToLower();
var dbConnStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

void ConfigureDbContext(DbContextOptionsBuilder opts)
{
    switch (dbType)
    {
        case "sqlserver" or "mssql":
            var sqlServerConn = dbConnStr
                ?? $"Server={Env("DB_HOST","localhost")},{Env("DB_PORT","1433")};Database={Env("DB_NAME","juggle")};User Id={Env("DB_USER","sa")};Password={Env("DB_PASS","Juggle@2026")};TrustServerCertificate=True;";
            opts.UseSqlServer(sqlServerConn);
            break;
        case "mysql":
            var mysqlConn = dbConnStr
                ?? $"Server={Env("DB_HOST","localhost")};Port={Env("DB_PORT","3306")};Database={Env("DB_NAME","juggle")};User={Env("DB_USER","root")};Password={Env("DB_PASS","juggle")};CharSet=utf8mb4;";
            opts.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn),
                mySql => { mySql.EnableRetryOnFailure(3); });
            break;
        case "postgresql" or "postgres":
            var pgConn = dbConnStr
                ?? $"Host={Env("DB_HOST","localhost")};Port={Env("DB_PORT","5432")};Database={Env("DB_NAME","juggle")};Username={Env("DB_USER","postgres")};Password={Env("DB_PASS","juggle")};";
            opts.UseNpgsql(pgConn);
            break;
        default: // sqlite
            var dbPath = dbConnStr
                ?? Environment.GetEnvironmentVariable("DB_PATH")
                ?? Path.Combine(Directory.GetCurrentDirectory(), "juggle.db");
            opts.UseSqlite($"Data Source={dbPath}");
            break;
    }
}

// 临时辅助函数
static string Env(string key, string defaultValue) => Environment.GetEnvironmentVariable(key) ?? defaultValue;

/// <summary>隐藏连接字符串中的密码部分</summary>
static string MaskPassword(string connStr)
{
    if (string.IsNullOrEmpty(connStr)) return connStr;
    return System.Text.RegularExpressions.Regex.Replace(
        connStr,
        @"(Password|PWD|User Id.*?Password)\s*=\s*[^;]+",
        m => m.Value.Contains('=') ? m.Value.Substring(0, m.Value.IndexOf('=') + 1) + "***" : m.Value,
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );
}

builder.Services.AddDbContext<JuggleDbContext>(ConfigureDbContext);

// 如果不是 SQLite，则需要在启动时确保数据库已迁移
// SQLite 使用 EnsureCreated，其他数据库使用 Migrate 或 EnsureCreated

// JWT 认证
var jwtKey = builder.Configuration["Jwt:Key"] ?? "JuggleNet6SecretKey2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = "JuggleNet6",
            ValidateAudience         = true,
            ValidAudience            = "JuggleNet6",
            ValidateLifetime         = true
        };
        // 统一返回 JSON 格式的 401，方便前端处理
        opts.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        new { code = 401, message = "未授权，请先登录", data = (object?)null },
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                        }
                    ));
            }
        };
    });

builder.Services.AddAuthorization();

// HttpClient（供流程引擎和 API 调试使用）
builder.Services.AddHttpClient();

// 流程引擎（Scoped，因依赖 HttpClientFactory）
builder.Services.AddScoped<FlowEngine>();

// 定时任务调度器（后台服务）
builder.Services.AddHostedService<Juggle.Api.Services.ScheduleTaskService>();

// ── 业务 Service 层 ──────────────────────────────────────────────
builder.Services.AddScoped<FlowExecutionService>();  // 流程执行核心（数据源、静态变量、日志）
builder.Services.AddScoped<DataSourceService>();     // 数据源连接字符串构建 + 连接测试
builder.Services.AddScoped<JwtService>();            // JWT Token 签发

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Juggle Net8 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Format: Bearer {token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS（开发环境允许前端跨域）
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ==================== 应用构建 ====================
var app = builder.Build();

// 启动日志：显示数据库类型
Console.WriteLine($"[Juggle] 系统数据库类型: {dbType.ToUpper()}");
if (!string.IsNullOrEmpty(dbConnStr))
    Console.WriteLine($"[Juggle] 连接字符串: {MaskPassword(dbConnStr)}");

// ==================== 数据库初始化 ====================
// SQLite: EnsureCreated 自动建表
// SQLServer/MySQL/PostgreSQL: EnsureCreated 自动建表（生产环境建议改用 Migration）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JuggleDbContext>();
    db.Database.EnsureCreated();

    // 补建迭代新增字段/表（仅 SQLite 需要手动 ALTER TABLE，其他数据库由 Migration 处理）
    if (dbType == "sqlite")
    {
        // 补建流程定义分组字段
        try { db.Database.ExecuteSqlRaw("ALTER TABLE t_flow_definition ADD COLUMN group_name TEXT DEFAULT NULL;"); }
        catch { /* 列已存在则忽略 */ }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 生产/容器环境同样开启 Swagger（可按需关闭）
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Juggle Net8 API v1"));
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// 前端静态文件（生产环境）
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// 健康检查端点（供 Docker healthcheck / 负载均衡探针使用）
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }))
   .AllowAnonymous();

// SPA fallback（前端路由）
app.MapFallbackToFile("index.html");

app.Run();
