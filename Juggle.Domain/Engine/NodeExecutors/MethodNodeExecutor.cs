using System.Text.Json;

namespace Juggle.Domain.Engine.NodeExecutors;

/// <summary>
/// METHOD 节点执行器：根据 FillRule 填充参数，发起 HTTP 请求，将结果写回变量
/// </summary>
public class MethodNodeExecutor : INodeExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MethodNodeExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> ExecuteAsync(FlowNode node, FlowContext context)
    {
        if (node.Method == null)
            throw new InvalidOperationException($"METHOD node [{node.Key}] has no method config.");

        var method = node.Method;
        var client = _httpClientFactory.CreateClient();

        // 1. 根据 inputFillRules 构建入参
        var inputParams = new Dictionary<string, object?>();
        var headerParams = new Dictionary<string, string>();

        foreach (var rule in method.HeaderFillRules)
        {
            var val = ResolveSource(rule, context);
            headerParams[rule.Target] = val?.ToString() ?? "";
        }

        foreach (var rule in method.InputFillRules)
        {
            var val = ResolveSource(rule, context);
            inputParams[rule.Target] = val;
        }

        // 2. 发起 HTTP 请求
        string responseJson;
        try
        {
            var requestType = method.RequestType?.ToUpper() ?? "GET";
            var url = method.Url;

            // 添加 headers
            foreach (var h in headerParams)
                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);

            if (requestType == "GET" || requestType == "DELETE")
            {
                // GET/DELETE 将参数拼接到 URL 查询字符串
                if (inputParams.Count > 0)
                {
                    var query = string.Join("&", inputParams.Select(kv =>
                        $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));
                    url = url.Contains('?') ? $"{url}&{query}" : $"{url}?{query}";
                }

                var resp = requestType == "GET"
                    ? await client.GetAsync(url)
                    : await client.DeleteAsync(url);
                responseJson = await resp.Content.ReadAsStringAsync();
            }
            else
            {
                // POST/PUT 发送 JSON body
                var content = new StringContent(
                    JsonSerializer.Serialize(inputParams),
                    System.Text.Encoding.UTF8,
                    "application/json");
                var resp = requestType == "PUT"
                    ? await client.PutAsync(url, content)
                    : await client.PostAsync(url, content);
                responseJson = await resp.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"METHOD node [{node.Key}] HTTP call failed: {ex.Message}", ex);
        }

        // 3. 解析响应，根据 outputFillRules 写回变量
        try
        {
            var responseDoc = JsonSerializer.Deserialize<JsonElement>(responseJson);
            foreach (var rule in method.OutputFillRules)
            {
                if (rule.TargetType == "VARIABLE")
                {
                    var value = ExtractJsonValue(responseDoc, rule.Source);
                    context.SetVariable(rule.Target, value);
                }
            }
        }
        catch
        {
            // 响应不是JSON，忽略 outputFillRules
        }

        return node.Outgoings.FirstOrDefault();
    }

    private static object? ResolveSource(FillRule rule, FlowContext context)
    {
        if (rule.SourceType == "CONSTANT") return rule.Source;
        return context.GetVariable(rule.Source);
    }

    private static object? ExtractJsonValue(JsonElement doc, string path)
    {
        // 支持简单 path，如 "data.token" 或 "loginFlag"
        var parts = path.Split('.');
        JsonElement current = doc;
        foreach (var part in parts)
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var next))
                current = next;
            else
                return null;
        }
        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.TryGetInt64(out var l) ? l : current.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => current.ToString()
        };
    }
}
