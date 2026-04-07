using Juggle.Application.Models.Request;
using Juggle.Application.Models.Response;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

[ApiController]
[Route("api/system/config")]
[Authorize]
public class SystemConfigController : ControllerBase
{
    private readonly JuggleDbContext _db;

    // 预定义的配置项（首次访问时自动初始化）
    private static readonly List<(string key, string name, string group, string defaultVal)> DefaultConfigs = new()
    {
        // 邮件配置
        ("email.smtp.host",     "SMTP服务器地址",     "email", "smtp.example.com"),
        ("email.smtp.port",     "SMTP端口",            "email", "465"),
        ("email.smtp.ssl",      "启用SSL",             "email", "true"),
        ("email.smtp.username", "SMTP用户名",          "email", ""),
        ("email.smtp.password", "SMTP密码",            "email", ""),
        ("email.from.address",  "发件人地址",           "email", ""),
        ("email.from.name",     "发件人名称",           "email", "Juggle告警"),
        // 告警配置
        ("alert.enabled",          "启用全局告警",     "alert", "false"),
        ("alert.webhook.url",      "告警Webhook地址",  "alert", ""),
        ("alert.webhook.secret",   "告警Webhook密钥",  "alert", ""),
        ("alert.email.to",         "告警收件人",        "alert", ""),
        ("alert.on.fail.enabled",  "流程失败时告警",    "alert", "true"),
        ("alert.min.cost.ms",      "慢执行告警阈值(ms)","alert", "0"),
        // 系统配置
        ("system.page.size",    "默认分页大小",  "system", "10"),
        ("system.log.keep.days","日志保留天数",  "system", "30"),
    };

    public SystemConfigController(JuggleDbContext db)
    {
        _db = db;
    }

    [HttpGet("all")]
    public async Task<ApiResult> GetAll()
    {
        // 确保默认配置项存在
        await EnsureDefaultsAsync();

        var configs = await _db.SystemConfigs
            .Where(c => c.Deleted == 0)
            .OrderBy(c => c.ConfigGroup)
            .ThenBy(c => c.Id)
            .ToListAsync();

        // 按分组返回
        var grouped = configs.GroupBy(c => c.ConfigGroup ?? "system")
            .ToDictionary(g => g.Key, g => g.Select(c => new
            {
                c.Id, c.ConfigKey, c.ConfigName, c.ConfigValue, c.Remark
            }).ToList());

        return ApiResult.Success(grouped);
    }

    [HttpPost("save")]
    public async Task<ApiResult> Save([FromBody] List<SystemConfigSaveRequest> items)
    {
        foreach (var item in items)
        {
            var cfg = await _db.SystemConfigs
                .FirstOrDefaultAsync(c => c.ConfigKey == item.ConfigKey && c.Deleted == 0);
            if (cfg != null)
            {
                cfg.ConfigValue = item.ConfigValue;
                cfg.UpdatedAt   = DateTime.Now.ToString("o");
            }
        }
        await _db.SaveChangesAsync();
        return ApiResult.Success();
    }

    private async Task EnsureDefaultsAsync()
    {
        foreach (var (key, name, group, defVal) in DefaultConfigs)
        {
            var exists = await _db.SystemConfigs.AnyAsync(c => c.ConfigKey == key && c.Deleted == 0);
            if (!exists)
            {
                _db.SystemConfigs.Add(new SystemConfigEntity
                {
                    ConfigKey   = key,
                    ConfigName  = name,
                    ConfigGroup = group,
                    ConfigValue = defVal,
                    Deleted     = 0,
                    CreatedAt   = DateTime.Now.ToString("o")
                });
            }
        }
        await _db.SaveChangesAsync();
    }
}
