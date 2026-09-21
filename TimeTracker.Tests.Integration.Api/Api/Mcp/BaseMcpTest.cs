using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class BaseMcpTest : BaseTest
{
    protected readonly string McpUrl = ApiUrl.Mcp;
    protected readonly IJwtAuthService JwtAuthService;

    public BaseMcpTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        JwtAuthService = ServiceProvider.GetRequiredService<IJwtAuthService>();
    }

    protected async Task<(HttpResponseMessage Response, McpCallToolResultDto? ToolResult, T? Data)> CallToolAsync<T>(
        string toolName,
        object? arguments = null,
        string? token = null
    )
    {
        await FlushDbChanges();
        using var message = new HttpRequestMessage(HttpMethod.Post, McpUrl);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        if (!string.IsNullOrEmpty(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var rpcRequest = new McpJsonRpcRequest
        {
            Id = 1,
            Method = "tools/call",
            Params = JsonSerializer.SerializeToElement(new
            {
                name = toolName,
                arguments = arguments ?? new { }
            })
        };
        message.Content = JsonContent.Create(rpcRequest);

        var response = await HttpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
        {
            return (response, null, default);
        }

        var content = await response.Content.ReadAsStringAsync();
        var jsonRpcData = content;
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("data: "))
            {
                jsonRpcData = trimmed["data: ".Length..].Trim();
                break;
            }
        }

        var rpcResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(jsonRpcData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (rpcResponse?.Result == null)
        {
            return (response, null, default);
        }

        var resultJson = JsonSerializer.Serialize(rpcResponse.Result);
        var toolResult = JsonSerializer.Deserialize<McpCallToolResultDto>(resultJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (toolResult?.Content == null || toolResult.Content.Count == 0)
        if (toolResult?.Content == null || toolResult.Content.Count == 0 || toolResult.IsError)
        {
            return (response, toolResult, default);
        }

        var rawText = toolResult.Content[0].Text;
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        serializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var data = JsonSerializer.Deserialize<T>(rawText, serializerOptions);

        return (response, toolResult, data);
    }
}
