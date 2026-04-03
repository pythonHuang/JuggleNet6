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

// SQLite + EF Core（支持 DB_PATH 环境变量，容器部署时指向挂载卷）
var dbPath = Environment.GetEnvironmentVariable("DB_PATH")
          ?? Path.Combine(Directory.GetCurrentDirectory(), "juggle.db");
builder.Services.AddDbContext<JuggleDbContext>(opts =>
    opts.UseSqlite($"Data Source={dbPath}"));

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

// 自动迁移数据库：EnsureCreated 仅在首次创建时建表
// 对于已存在的数据库，补建后续迭代新增的表（幂等 CREATE TABLE IF NOT EXISTS）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JuggleDbContext>();
    db.Database.EnsureCreated();

    // 补建迭代四新增的三张表（若已存在则跳过）
    db.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS t_flow_log (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    deleted      INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT,
    created_by   TEXT,
    updated_at   TEXT,
    updated_by   TEXT,
    flow_key     TEXT,
    flow_name    TEXT,
    version      INTEGER,
    trigger_type TEXT,
    status       TEXT,
    start_time   TEXT,
    end_time     TEXT,
    cost_ms      INTEGER,
    error_message TEXT,
    input_json   TEXT,
    output_json  TEXT
);");

    db.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS t_flow_node_log (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    deleted         INTEGER NOT NULL DEFAULT 0,
    created_at      TEXT,
    created_by      TEXT,
    updated_at      TEXT,
    updated_by      TEXT,
    flow_log_id     INTEGER,
    node_key        TEXT,
    node_label      TEXT,
    node_type       TEXT,
    seq_no          INTEGER,
    status          TEXT,
    start_time      TEXT,
    end_time        TEXT,
    cost_ms         INTEGER,
    input_snapshot  TEXT,
    output_snapshot TEXT,
    detail          TEXT,
    error_message   TEXT
);");

    db.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS t_static_variable (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    deleted       INTEGER NOT NULL DEFAULT 0,
    created_at    TEXT,
    created_by    TEXT,
    updated_at    TEXT,
    updated_by    TEXT,
    var_code      TEXT NOT NULL,
    var_name      TEXT,
    data_type     TEXT,
    value         TEXT,
    default_value TEXT,
    description   TEXT,
    group_name    TEXT
);");

    // 补建流程定义分组字段
    try { db.Database.ExecuteSqlRaw("ALTER TABLE t_flow_definition ADD COLUMN group_name TEXT DEFAULT NULL;"); }
    catch { /* 列已存在则忽略 */ }
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
