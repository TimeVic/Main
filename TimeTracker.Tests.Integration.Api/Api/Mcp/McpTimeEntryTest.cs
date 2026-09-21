using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using Xunit;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class McpTimeEntryTest : BaseMcpTest
{
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _mcpJwtToken;
    private readonly IProjectSeeder _projectSeeder;

    public McpTimeEntryTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
        _mcpJwtToken = JwtAuthService.BuildMcpJwt(_user.Id, _workspace.Id);
        _mcpJwtToken = JwtAuthService.BuildMcpJwt(_user.Id);
    }

    [Fact]
    public async Task TimeEntryStart_StartsNewActiveEntry()
    public async Task TimeEntrySet_CreatesTimeEntryWithStartAndEndTimes()
    {
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, 1);
        var project = projects.First();

        var (response, toolResult, actual) = await CallToolAsync<StartResponse>(
            "time_entry_start",
        var startTime = DateTime.UtcNow.Date.AddHours(9);
        var endTime = DateTime.UtcNow.Date.AddHours(11);

        var (response, toolResult, actual) = await CallToolAsync<TimeEntryDto>(
            "time_entry_set",
            arguments: new
            {
                description = "Developing MCP feature",
                workspaceId = _workspace.Id,
                startTime = startTime,
                endTime = endTime,
                projectId = project.Id,
                description = "Manual entry for 2 hours",
                isBillable = true,
                hourlyRate = 50m
                hourlyRate = 60m
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(toolResult);
        Assert.False(toolResult.IsError);
        Assert.NotNull(actual);
        Assert.NotNull(actual.ActiveTimeEntry);
        Assert.Equal("Developing MCP feature", actual.ActiveTimeEntry.Description);
        Assert.Equal(project.Id, actual.ActiveTimeEntry.Project?.Id);
        Assert.True(actual.ActiveTimeEntry.IsBillable);
        Assert.Equal(50m, actual.ActiveTimeEntry.HourlyRate);
        Assert.Null(actual.ActiveTimeEntry.EndTime);
        Assert.Null(actual.StoppedTimeEntry);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal("Manual entry for 2 hours", actual.Description);
        Assert.Equal(startTime, actual.StartTime);
        Assert.Equal(endTime, actual.EndTime);
        Assert.Equal(project.Id, actual.Project?.Id);
        Assert.True(actual.IsBillable);
        Assert.Equal(60m, actual.HourlyRate);
    }

    [Fact]
    public async Task TimeEntryStart_WhenActiveExists_StopsPreviousAndStartsNew()
    public async Task TimeEntrySet_CreatesActiveEntryWhenEndTimeIsNull()
    {
        // 1. Start first entry
        var (_, _, firstResult) = await CallToolAsync<StartResponse>(
            "time_entry_start",
            arguments: new { description = "First Task" },
            token: _mcpJwtToken
        );
        Assert.NotNull(firstResult?.ActiveTimeEntry);
        var startTime = DateTime.UtcNow.Date.AddHours(12);

        // 2. Start second entry
        var (response, toolResult, secondResult) = await CallToolAsync<StartResponse>(
            "time_entry_start",
            arguments: new { description = "Second Task" },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(secondResult);
        Assert.NotNull(secondResult.ActiveTimeEntry);
        Assert.Equal("Second Task", secondResult.ActiveTimeEntry.Description);
        Assert.Null(secondResult.ActiveTimeEntry.EndTime);

        Assert.NotNull(secondResult.StoppedTimeEntry);
        Assert.Equal("First Task", secondResult.StoppedTimeEntry.Description);
        Assert.NotNull(secondResult.StoppedTimeEntry.EndTime);
    }

    [Fact]
    public async Task TimeEntryStop_StopsActiveEntry()
    {
        await CallToolAsync<StartResponse>(
            "time_entry_start",
            arguments: new { description = "Work to be stopped" },
            token: _mcpJwtToken
        );

        var (response, toolResult, actual) = await CallToolAsync<TimeEntryDto>(
            "time_entry_stop",
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.Equal("Work to be stopped", actual.Description);
        Assert.NotNull(actual.EndTime);
    }

    [Fact]
    public async Task TimeEntrySet_CreatesTimeEntryWithStartAndEndTimes()
    {
        var startTime = DateTime.UtcNow.Date.AddHours(9);
        var endTime = DateTime.UtcNow.Date.AddHours(11);

        var (response, toolResult, actual) = await CallToolAsync<TimeEntryDto>(
            "time_entry_set",
            arguments: new
            {
                workspaceId = _workspace.Id,
                startTime = startTime,
                endTime = endTime,
                description = "Manual entry for 2 hours",
                isBillable = false
                description = "Active ongoing work"
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal("Manual entry for 2 hours", actual.Description);
        Assert.Equal(startTime, actual.StartTime);
        Assert.Equal(endTime, actual.EndTime);
        Assert.Null(actual.EndTime);
        Assert.Equal("Active ongoing work", actual.Description);
    }

    [Fact]
    public async Task TimeEntrySet_UpdatesExistingEntry()
    {
        var startTime = DateTime.UtcNow.Date.AddHours(14);
        var endTime = DateTime.UtcNow.Date.AddHours(16);

        var (_, _, created) = await CallToolAsync<TimeEntryDto>(
            "time_entry_set",
            arguments: new
            {
                workspaceId = _workspace.Id,
                startTime = startTime,
                endTime = endTime,
                description = "Initial description"
            },
            token: _mcpJwtToken
        );
        Assert.NotNull(created);

        var (response, _, updated) = await CallToolAsync<TimeEntryDto>(
            "time_entry_set",
            arguments: new
            {
                workspaceId = _workspace.Id,
                id = created.Id,
                startTime = startTime,
                endTime = endTime,
                description = "Updated description"
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task TimeEntrySet_WithoutAccessToWorkspace_ReturnsError()
    {
        var (_, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var startTime = DateTime.UtcNow.Date.AddHours(9);

        var (response, toolResult, actual) = await CallToolAsync<TimeEntryDto>(
            "time_entry_set",
            arguments: new
            {
                workspaceId = otherWorkspace.Id,
                startTime = startTime,
                description = "Should fail"
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(toolResult);
        Assert.True(toolResult.IsError);
    }
}
