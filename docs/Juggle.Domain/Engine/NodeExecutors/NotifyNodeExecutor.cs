using System.Text;
using System.Text.Json;

namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// NOTIFY 通知节点执行器：在流程中发送 Webhook 回调或邮件通知。
/// 
/// 支持的通知类型：
/// - WEBHOOK：向指定 URL 发送 HTTP 请求（POST/GET），请求体支持 ${varName} 变量模板替换
/// - EMAIL：发送邮件（需要配置 SMTP，当前仅记录日志，后续可扩展）
/// 
/// 通知失败行为由 failOnError 控制：
/// - true：通知失败抛出异常，中断流程
/// - false：通知失败仅记录日志，流程继续执行
/// </summary>
public class NotifyNodeExecutor : INodeExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NotifyNodeExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        var cfg = node.NotifyConfig;
        if (cfg == null)
            throw new InvalidOperationException($"NOTIFY node [{node.Key}] 未配置 notifyConfig");

        var notifyType = (cfg.NotifyType ?? "WEBHOOK").ToUpper();
        var errorPrefix = $"NOTIFY node [{node.Key}]";

        try
        {
            if (notifyType == "WEBHOOK")
            {
                await ExecuteWebhook(cfg, context, errorPrefix);
            }
            else if (notifyType == "EMAIL")
            {
                // EMAIL 暂仅记录日志，后续可集成 SMTP
                var emailTo = ReplaceVariables(cfg.EmailTo ?? "", context);
                var subject = ReplaceVariables(cfg.EmailSubject ?? "流程通知", context);
                var body = ReplaceVariables(cfg.BodyTemplate, context);

                // 记录到上下文日志
                context.AddLog($"{errorPrefix}: EMAIL通知（收件人: {emailTo}, 主题: {subject}）- SMTP暂未集成，仅记录日志");
            }
            else
            {
                throw new InvalidOperationException($"{errorPrefix}: 未知通知类型 [{notifyType}]，支持 WEBHOOK/EMAIL");
            }
        }
        catch (Exception ex) when (!cfg.FailOnError)
        {
            // failOnError=false 时仅记录错误，不中断流程
            context.AddLog($"{errorPrefix}: 通知失败（已忽略）: {ex.Message}");
        }

        return node.Outgoings.FirstOrDefault();
    }

    private async Task ExecuteWebhook(NotifyConfig cfg, FlowContext context, string errorPrefix)
    {
        var url = ReplaceVariables(cfg.WebhookUrl, context);
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException($"{errorPrefix}: Webhook URL 为空");

        var method = (cfg.WebhookMethod ?? "POST").ToUpper();
        var body = ReplaceVariables(cfg.BodyTemplate, context);

        var client = _httpClientFactory.CreateClient("notify");
        client.Timeout = TimeSpan.FromSeconds(30);

        var request = new HttpRequestMessage(
            method == "GET" ? HttpMethod.Get : HttpMethod.Post,
            url);

        // 添加自定义请求头
        if (!string.IsNullOrWhiteSpace(cfg.WebhookHeaders))
        {
            try
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(cfg.WebhookHeaders);
                if (headers != null)
                {
                    foreach (var h in headers)
                        request.Headers.TryAddWithoutValidation(h.Key, ReplaceVariables(h.Value, context));
                }
            }
            catch
            {
                // 请求头解析失败，跳过
            }
        }

        // 请求体
        if (method != "GET" && !string.IsNullOrWhiteSpace(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"{errorPrefix}: Webhook 请求失败, HTTP {(int)response.StatusCode}, 响应: {responseContent}");
        }

        context.AddLog($"{errorPrefix}: Webhook 通知成功, HTTP {(int)response.StatusCode}");
    }

    /// <summary>替换模板中的 ${varName} 为流程变量值</summary>
    private static string ReplaceVariables(string template, FlowContext context)
    {
        if (string.IsNullOrEmpty(template)) return template;
        return System.Text.RegularExpressions.Regex.Replace(template, @"\$\{(\w+)\}", match =>
        {
            var varName = match.Groups[1].Value;
            var value = context.GetVariable(varName);
            return value?.ToString() ?? match.Value;
        });
    }
}
