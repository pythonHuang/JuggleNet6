using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Juggle.Application.Models.Response;
using Juggle.Application.Services.Flow;
using Juggle.Domain.Entities;
using Juggle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juggle.Api.Controllers.Open;

/// <summary>
/// Webhook 触发开放接口：外部系统通过 HTTP 请求触发流程执行。
/// URL 格式：POST/GET /open/webhook/{webhookKey}
/// </summary>
[ApiController]
[Route("open/webhook")]
public class WebhookTriggerController : ControllerBase
{
    private readonly JuggleDbContext _db;
    private readonly FlowExecutionService _flowExec;

    public WebhookTriggerController(JuggleDbContext db, FlowExecutionService flowExec)
    {
        _db = db;
        _flowExec = flowExec;
    }

    /// <summary>POST 触发 Webhook</summary>
    [HttpPost("{webhookKey}")]
    public async Task<ApiResult> TriggerPost(
        string webhookKey,
        [FromBody] Dictionary<string, object?> bodyParams,
        [FromHeader(Name = "X-Webhook-Signature")] string? signature)
    {
        return await TriggerWebhook(webhookKey, bodyParams, signature);
    }

    /// <summary>GET 触发 Webhook</summary>
    [HttpGet("{webhookKey}")]
    public async Task<ApiResult> TriggerGet(
        string webhookKey,
        [FromQuery] Dictionary<string, string> queryParams,
        [FromHeader(Name = "X-Webhook-Signature")] string? signature)
    {
        return await TriggerWebhook(webhookKey,
            queryParams.ToDictionary(k => k.Key, k => (object?)k.Value), signature);
    }

    private async Task<ApiResult> TriggerWebhook(
        string webhookKey,
        Dictionary<string, object?> inputParams,
        string? signature)
    {
        // 1. 查找 Webhook 配置
        var webhook = await _db.Webhooks
            .FirstOrDefaultAsync(w => w.WebhookKey == webhookKey && w.Status == 1 && w.Deleted == 0);

        if (webhook == null)
            return ApiResult.Fail("Webhook 不存在或已禁用", 404);

        // 2. 验证签名（如果配置了密钥）
        if (!string.IsNullOrEmpty(webhook.Secret))
        {
            if (string.IsNullOrEmpty(signature))
                return ApiResult.Fail("缺少签名（X-Webhook-Signature Header）", 403);

            var bodyJson = JsonSerializer.Serialize(inputParams);
            var expectedSig = ComputeHmacSha256(bodyJson, webhook.Secret);
            if (!string.Equals(signature, expectedSig, StringComparison.OrdinalIgnoreCase))
                return ApiResult.Fail("签名验证失败", 403);
        }

        // 3. 查找已发布流程版本
        var flowVersion = await _db.FlowVersions
            .Where(v => v.FlowKey == webhook.FlowKey && v.Status == 1 && v.Deleted == 0)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync();

        if (flowVersion == null)
            return ApiResult.Fail("关联流程未发布或已禁用");

        var definition = await _db.FlowDefinitions
            .FirstOrDefaultAsync(f => f.FlowKey == webhook.FlowKey && f.Deleted == 0);

        if (definition == null)
            return ApiResult.Fail("流程定义不存在");

        // 4. 更新触发统计
        webhook.TriggerCount++;
        webhook.LastTriggerTime = DateTime.Now.ToString("o");
        _db.SaveChanges();

        // 5. 执行流程
        if (webhook.AsyncMode == 1)
        {
            // 异步模式：立即返回 logId
            var logId = await _flowExec.CreateRunningLogAsync(definition, flowVersion.Version!, inputParams);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _flowExec.RunAsyncWithLog(definition, flowVersion.FlowContent!, inputParams,
                        "webhook", flowVersion.Version!, logId);
                }
                catch { }
            });

            return ApiResult.Success(new { logId, message = "Webhook 已提交异步执行" });
        }
        else
        {
            // 同步模式：等待执行完成
            var result = await _flowExec.RunAsync(
                definition, flowVersion.FlowContent!, inputParams, "webhook", flowVersion.Version!);

            return result.Success
                ? ApiResult.Success(result.OutputData)
                : ApiResult.Fail(result.ErrorMessage ?? "执行失败");
        }
    }

    /// <summary>计算 HMAC-SHA256 签名</summary>
    private static string ComputeHmacSha256(string data, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
