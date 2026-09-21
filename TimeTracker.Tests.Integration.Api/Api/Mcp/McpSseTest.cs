using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Model.Mcp.Rpc;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using Xunit;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class McpSseTest : BaseTest
{
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _mcpJwtToken;
    private readonly IJwtAuthService _jwtAuthService;

    public McpSseTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtAuthService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
        _mcpJwtToken = _jwtAuthService.BuildMcpJwt(_user.Id, _workspace.Id);
        _mcpJwtToken = _jwtAuthService.BuildMcpJwt(_user.Id);
    }

    [Fact]
    public async Task Anonymous_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/mcp");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Content = JsonContent.Create(new { });
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StreamableHttp_PostToolCall_StreamsSseResponse()
    {
        await FlushDbChanges();
        using var client = _factory.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

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

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json")
        };
        postRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        postRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _mcpJwtToken);

        var response = await client.SendAsync(postRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        string? jsonRpcData = null;
        string? line;
        while ((line = await reader.ReadLineAsync(cts.Token)) != null)
        {
            if (line.StartsWith("data: "))
            {
                var data = line["data: ".Length..].Trim();
                if (data.Contains("\"jsonrpc\""))
                {
                    jsonRpcData = data;
                    break;
                }
            }
        }

        Assert.NotNull(jsonRpcData);
        var rpcResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(jsonRpcData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(rpcResponse);
        Assert.Null(rpcResponse.Error);
        Assert.NotNull(rpcResponse.Result);
    }
}
