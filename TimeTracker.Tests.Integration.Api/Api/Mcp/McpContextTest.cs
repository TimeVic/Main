using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Model.Mcp.Context;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using Xunit;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class McpContextTest : BaseMcpTest
{
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _mcpJwtToken;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceSeeder _workspaceSeeder;

    public McpContextTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
        _mcpJwtToken = JwtAuthService.BuildMcpJwt(_user.Id);
    }

    [Fact]
    public async Task GetUserContext_ReturnsUserWorkspacesAndProjectsWithWorkspaceReference()
    {
        var ws1Projects = await _projectSeeder.CreateSeveralAsync(_workspace, 2);
        var secondWorkspaces = await _workspaceSeeder.CreateSeveralAsync(_user, 1);
        var ws2 = secondWorkspaces.First();
        var ws2Projects = await _projectSeeder.CreateSeveralAsync(ws2, 2);

        var (response, toolResult, actual) = await CallToolAsync<UserContextDto>(
            "get_user_context",
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(toolResult);
        Assert.False(toolResult.IsError);
        Assert.NotNull(actual);

        // Verify user profile
        Assert.Equal(_user.Id, actual.User.Id);
        Assert.Equal(_user.Email, actual.User.Email);

        // Verify workspaces list
        Assert.Contains(actual.Workspaces, w => w.Id == _workspace.Id);
        Assert.Contains(actual.Workspaces, w => w.Id == ws2.Id);

        // Verify projects and their workspace association
        foreach (var p in ws1Projects)
        {
            var found = actual.Projects.FirstOrDefault(item => item.Id == p.Id);
            Assert.NotNull(found);
            Assert.NotNull(found.Workspace);
            Assert.Equal(_workspace.Id, found.Workspace.Id);
        }

        foreach (var p in ws2Projects)
        {
            var found = actual.Projects.FirstOrDefault(item => item.Id == p.Id);
            Assert.NotNull(found);
            Assert.NotNull(found.Workspace);
            Assert.Equal(ws2.Id, found.Workspace.Id);
        }
    }

    [Fact]
    public async Task GetUserContext_DoesNotReturnWorkspacesOrProjectsFromOtherUsers()
    {
        var myProjects = await _projectSeeder.CreateSeveralAsync(_workspace, 2);

        var (_, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProjects = await _projectSeeder.CreateSeveralAsync(otherWorkspace, 3);

        var (response, toolResult, actual) = await CallToolAsync<UserContextDto>(
            "get_user_context",
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);

        // Does not contain other user's workspace
        Assert.DoesNotContain(actual.Workspaces, w => w.Id == otherWorkspace.Id);

        // Does not contain other user's projects
        foreach (var otherProject in otherProjects)
        {
            Assert.DoesNotContain(actual.Projects, p => p.Id == otherProject.Id);
        }

        // Contains own projects
        foreach (var myProject in myProjects)
        {
            Assert.Contains(actual.Projects, p => p.Id == myProject.Id);
        }
    }

    [Fact]
    public async Task ProjectList_WithWorkspaceId_ReturnsProjectsForSpecifiedWorkspace()
    {
        var ws1Projects = await _projectSeeder.CreateSeveralAsync(_workspace, 2);
        var secondWorkspaces = await _workspaceSeeder.CreateSeveralAsync(_user, 1);
        var ws2 = secondWorkspaces.First();
        var ws2Projects = await _projectSeeder.CreateSeveralAsync(ws2, 2);

        var (response, toolResult, actual) = await CallToolAsync<GetListResponse>(
            "project_list",
            arguments: new { workspaceId = _workspace.Id },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.Equal(2, actual.TotalCount);

        foreach (var p in ws1Projects)
        {
            Assert.Contains(actual.Items, item => item.Id == p.Id);
        }

        foreach (var p in ws2Projects)
        {
            Assert.DoesNotContain(actual.Items, item => item.Id == p.Id);
        }
    }
}

