using JuggleNet6.Backend.Domain.Engine;
using JuggleNet6.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ==================== 服务注册 ====================

// JSON 配置（camelCase 输出）
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// SQLite + EF Core
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "juggle.db");
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = "JuggleNet6",
            ValidateAudience = true,
            ValidAudience = "JuggleNet6",
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// HttpClient（供流程引擎和 API 调试使用）
builder.Services.AddHttpClient();

// 流程引擎（Scoped，因依赖 HttpClientFactory）
builder.Services.AddScoped<FlowEngine>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Juggle Net8 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
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

// 自动迁移数据库
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JuggleDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// 前端静态文件（生产环境）
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// SPA fallback（前端路由）
app.MapFallbackToFile("index.html");

app.Run();
