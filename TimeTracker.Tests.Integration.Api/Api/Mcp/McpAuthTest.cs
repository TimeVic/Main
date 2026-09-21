using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using Xunit;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class McpAuthTest : BaseTest
{
    private readonly string _url = ApiUrl.Mcp;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _webJwtToken;
    private readonly string _mcpJwtToken;
    private readonly IJwtAuthService _jwtAuthService;

    public McpAuthTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtAuthService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        (_webJwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _mcpJwtToken = _jwtAuthService.BuildMcpJwt(_user.Id, _workspace.Id);
        _mcpJwtToken = _jwtAuthService.BuildMcpJwt(_user.Id);
    }

    [Fact]
    public async Task AnonymousCannotCallTool()
    {
        var request = new McpJsonRpcRequest
        {
            Id = 1,
            Method = "tools/call",
            Params = JsonSerializer.SerializeToElement(new
            {
                name = "project_list",
                name = "get_user_context",
                arguments = new { }
            })
        };

        var response = await PostMcpAsync(null, request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InvalidTokenCannotCallTool()
    {
        var request = new McpJsonRpcRequest
        {
            Id = 1,
            Method = "tools/call",
            Params = JsonSerializer.SerializeToElement(new
            {
                name = "project_list",
                name = "get_user_context",
                arguments = new { }
            })
        };

        var response = await PostMcpAsync("invalid_jwt_token_value", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StandardWebJwtCannotCallTool_RejectedByMcpHandler()
    {
        // Standard Web JWT must be rejected because it lacks the MCP purpose and workspaceId claims
        // Standard Web JWT must be rejected because it lacks the MCP purpose claim
        var request = new McpJsonRpcRequest
        {
            Id = 1,
            Method = "tools/call",
            Params = JsonSerializer.SerializeToElement(new
            {
                name = "project_list",
                name = "get_user_context",
                arguments = new { }
            })
        };

        var response = await PostMcpAsync(_webJwtToken, request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidMcpTokenCanCallTool()
    {
        var request = new McpJsonRpcRequest
        {
            Id = 1,
            Method = "tools/call",
            Params = JsonSerializer.SerializeToElement(new
            {
                name = "project_list",
                name = "get_user_context",
                arguments = new { }
            })
        };

        var response = await PostMcpAsync(_mcpJwtToken, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await ExtractJsonFromResponseAsync(response);
        var rpcResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(rpcResponse);
        Assert.Null(rpcResponse.Error);
        Assert.NotNull(rpcResponse.Result);

        var resultJson = JsonSerializer.Serialize(rpcResponse.Result);
        var toolResult = JsonSerializer.Deserialize<McpCallToolResultDto>(resultJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(toolResult);
        Assert.False(toolResult.IsError);
        Assert.NotEmpty(toolResult.Content);

        var projectList = JsonSerializer.Deserialize<GetListResponse>(toolResult.Content[0].Text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(projectList);
    }

    [Fact]
    public async Task ValidMcpTokenCanListTools()
    {
        var request = new McpJsonRpcRequest
        {
            Id = 2,
            Method = "tools/list",
            Params = JsonSerializer.SerializeToElement(new { })
        };

        var response = await PostMcpAsync(_mcpJwtToken, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await ExtractJsonFromResponseAsync(response);
        var rpcResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(rpcResponse);
        Assert.Null(rpcResponse.Error);
        Assert.NotNull(rpcResponse.Result);

        var resultDoc = JsonDocument.Parse(JsonSerializer.Serialize(rpcResponse.Result));
        var tools = resultDoc.RootElement.GetProperty("tools");
        Assert.True(tools.GetArrayLength() >= 5);
        Assert.True(tools.GetArrayLength() >= 4);

        var toolNames = new List<string>();
        foreach (var t in tools.EnumerateArray())
        {
            toolNames.Add(t.GetProperty("name").GetString()!);
        }

        Assert.Contains("get_user_context", toolNames);
        Assert.Contains("project_list", toolNames);
        Assert.Contains("time_entry_start", toolNames);
        Assert.Contains("time_entry_stop", toolNames);
        Assert.Contains("time_entry_set", toolNames);
        Assert.Contains("get_summary_report", toolNames);
        Assert.DoesNotContain("time_entry_start", toolNames);
        Assert.DoesNotContain("time_entry_stop", toolNames);
    }

    private async Task<HttpResponseMessage> PostMcpAsync(string? token, object data)
    {
        await FlushDbChanges();
        using var message = new HttpRequestMessage(HttpMethod.Post, _url);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        if (!string.IsNullOrEmpty(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        message.Content = JsonContent.Create(data);
        return await HttpClient.SendAsync(message);
    }

    private static async Task<string> ExtractJsonFromResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("data: "))
            {
                return trimmed["data: ".Length..].Trim();
            }
        }
        return content;
    }
}
