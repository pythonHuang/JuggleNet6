using Juggle.Application.Models.Response;
using Juggle.Application.Services;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Api;

/// <summary>Webhook 管理接口</summary>
[ApiController]
[Route("api/system/webhook")]
[Authorize]
public class WebhookController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly ITenantAccessor _tenant;

    public WebhookController(JuggleDbContext db, ITenantAccessor tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>分页查询 Webhook 列表</summary>
    [HttpPost("page")]
    public async Task<ApiResult> Page([FromBody] PageRequest req)
    {
        var query = _db.Webhooks.Where(w => w.Deleted == 0);

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(w => w.WebhookName!.Contains(req.Keyword) || w.WebhookKey!.Contains(req.Keyword));

        var total = await query.CountAsync();
        var list = await query.OrderByDescending(w => w.Id)
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return ApiResult.Success(new { total, records = list });
    }

    /// <summary>获取所有已发布流程（下拉选择用）</summary>
    [HttpGet("published-flows")]
    public async Task<ApiResult> PublishedFlows()
    {
        var flows = await _db.FlowVersions
            .Where(v => v.Status == 1 && v.Deleted == 0)
            .GroupBy(v => v.FlowKey)
            .Select(g => new { flowKey = g.Key, flowName = g.OrderByDescending(v => v.Id).First().FlowContent })
            .ToListAsync();

        // 取最新版本的 flowName 从 FlowDefinition
        var result = new List<object>();
        foreach (var f in flows)
        {
            var def = await _db.FlowDefinitions.FirstOrDefaultAsync(d => d.FlowKey == f.flowKey && d.Deleted == 0);
            result.Add(new { flowKey = f.flowKey, flowName = def?.FlowName ?? f.flowKey });
        }

        return ApiResult.Success(result);
    }

    /// <summary>新建 Webhook</summary>
    [HttpPost("save")]
    public async Task<ApiResult> Save([FromBody] WebhookEntity webhook)
    {
        if (string.IsNullOrWhiteSpace(webhook.WebhookKey))
            return ApiResult.Fail("Webhook Key 不能为空");
        if (string.IsNullOrWhiteSpace(webhook.FlowKey))
            return ApiResult.Fail("关联流程不能为空");

        // 检查 webhookKey 唯一性
        var exists = await _db.Webhooks.AnyAsync(w => w.WebhookKey == webhook.WebhookKey && w.Deleted == 0);
        if (exists)
            return ApiResult.Fail("Webhook Key 已存在，请使用其他名称");

        // 获取流程名称
        var def = await _db.FlowDefinitions.FirstOrDefaultAsync(d => d.FlowKey == webhook.FlowKey && d.Deleted == 0);
        if (def != null) webhook.FlowName = def.FlowName;

        webhook.TenantId = _tenant.TenantId;
        webhook.CreatedAt = DateTime.Now.ToString("o");
        webhook.TriggerCount = 0;
        _db.Webhooks.Add(webhook);
        await _db.SaveChangesAsync();
        return ApiResult.Success(webhook);
    }

    /// <summary>更新 Webhook</summary>
    [HttpPut("update")]
    public async Task<ApiResult> Update([FromBody] WebhookEntity webhook)
    {
        if (webhook.Id <= 0)
            return ApiResult.Fail("ID 无效");

        var entity = await _db.Webhooks.FindAsync(webhook.Id);
        if (entity == null || entity.Deleted == 1)
            return ApiResult.Fail("Webhook 不存在");

        // 如果修改了 webhookKey，检查唯一性
        if (entity.WebhookKey != webhook.WebhookKey)
        {
            var exists = await _db.Webhooks.AnyAsync(w => w.WebhookKey == webhook.WebhookKey && w.Deleted == 0 && w.Id != webhook.Id);
            if (exists) return ApiResult.Fail("Webhook Key 已存在");
        }

        entity.WebhookKey = webhook.WebhookKey;
        entity.WebhookName = webhook.WebhookName;
        entity.FlowKey = webhook.FlowKey;
        entity.Secret = webhook.Secret;
        entity.AllowedMethod = webhook.AllowedMethod;
        entity.AsyncMode = webhook.AsyncMode;
        entity.Status = webhook.Status;
        entity.Remark = webhook.Remark;
        entity.UpdatedAt = DateTime.Now.ToString("o");

        // 更新流程名称
        if (!string.IsNullOrEmpty(webhook.FlowKey))
        {
            var def = await _db.FlowDefinitions.FirstOrDefaultAsync(d => d.FlowKey == webhook.FlowKey && d.Deleted == 0);
            if (def != null) entity.FlowName = def.FlowName;
        }

        await _db.SaveChangesAsync();
        return ApiResult.Success(entity);
    }

    /// <summary>删除 Webhook（逻辑删除）</summary>
    [HttpDelete("delete/{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        var entity = await _db.Webhooks.FindAsync(id);
        if (entity == null || entity.Deleted == 1)
            return ApiResult.Fail("Webhook 不存在");

        entity.Deleted = 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success("删除成功");
    }

    /// <summary>获取单个 Webhook 详情</summary>
    [HttpGet("{id}")]
    public async Task<ApiResult> Get(long id)
    {
        var entity = await _db.Webhooks.FindAsync(id);
        if (entity == null || entity.Deleted == 1)
            return ApiResult.Fail("Webhook 不存在");
        return ApiResult.Success(entity);
    }

    /// <summary>启用/禁用 Webhook</summary>
    [HttpPut("toggle/{id}")]
    public async Task<ApiResult> Toggle(long id)
    {
        var entity = await _db.Webhooks.FindAsync(id);
        if (entity == null || entity.Deleted == 1)
            return ApiResult.Fail("Webhook 不存在");

        entity.Status = entity.Status == 1 ? 0 : 1;
        entity.UpdatedAt = DateTime.Now.ToString("o");
        await _db.SaveChangesAsync();
        return ApiResult.Success(new { id = entity.Id, status = entity.Status });
    }
}

/// <summary>通用分页请求</summary>
public class PageRequest
{
    public int PageNum { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
}
