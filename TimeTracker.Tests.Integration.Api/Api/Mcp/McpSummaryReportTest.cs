using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using Xunit;

namespace TimeTracker.Tests.Integration.Api.Api.Mcp;

public class McpSummaryReportTest : BaseMcpTest
{
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly string _mcpJwtToken;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;

    public McpSummaryReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
        _mcpJwtToken = JwtAuthService.BuildMcpJwt(_user.Id, _workspace.Id);
        _mcpJwtToken = JwtAuthService.BuildMcpJwt(_user.Id);
    }

    [Fact]
    public async Task GetSummaryReport_GroupByDay_ReturnsAggregatedTime()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);

        var today = DateTime.UtcNow.Date;
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto
        {
            StartTime = today.AddHours(9),
            EndTime = today.AddHours(12),
            IsBillable = true,
            HourlyRate = 20
        }, project);

        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto
        {
            StartTime = today.AddHours(13),
            EndTime = today.AddHours(15),
            IsBillable = true,
            HourlyRate = 20
        }, project);

        var (response, toolResult, actual) = await CallToolAsync<SummaryReportResponse>(
            "get_summary_report",
            arguments: new
            {
                workspaceId = _workspace.Id,
                startTime = today,
                endTime = today.AddDays(1).AddTicks(-1),
                type = SummaryReportType.GroupByDay
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.NotEmpty(actual.ByDays);
        Assert.NotNull(actual.GroupedByDay);
        Assert.NotEmpty(actual.GroupedByDay);

        var dayItem = actual.ByDays.First();
        Assert.Equal(TimeSpan.FromHours(5), dayItem.Duration);
    }

    [Fact]
    public async Task GetSummaryReport_GroupByProject_ReturnsProjectBreakdown()
    {
        var projects = (await _projectSeeder.CreateSeveralAsync(_workspace, 2)).ToList();
        var project1 = projects[0];
        var project2 = projects[1];

        var today = DateTime.UtcNow.Date;
        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto
        {
            StartTime = today.AddHours(9),
            EndTime = today.AddHours(11),
            IsBillable = true
        }, project1);

        await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto
        {
            StartTime = today.AddHours(13),
            EndTime = today.AddHours(16),
            IsBillable = true
        }, project2);

        var (response, toolResult, actual) = await CallToolAsync<SummaryReportResponse>(
            "get_summary_report",
            arguments: new
            {
                workspaceId = _workspace.Id,
                startTime = today,
                endTime = today.AddDays(1).AddTicks(-1),
                type = SummaryReportType.GroupByProject
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actual);
        Assert.NotNull(actual.GroupedByProject);
        Assert.Equal(2, actual.GroupedByProject.Count);

        var item1 = actual.GroupedByProject.FirstOrDefault(p => p.ProjectId == project1.Id);
        Assert.NotNull(item1);
        Assert.Equal(TimeSpan.FromHours(2), item1.Duration);

        var item2 = actual.GroupedByProject.FirstOrDefault(p => p.ProjectId == project2.Id);
        Assert.NotNull(item2);
        Assert.Equal(TimeSpan.FromHours(3), item2.Duration);
    }

    [Fact]
    public async Task GetSummaryReport_WithoutAccessToWorkspace_ReturnsError()
    {
        var (_, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var today = DateTime.UtcNow.Date;

        var (response, toolResult, actual) = await CallToolAsync<SummaryReportResponse>(
            "get_summary_report",
            arguments: new
            {
                workspaceId = otherWorkspace.Id,
                startTime = today,
                endTime = today.AddDays(1).AddTicks(-1),
                type = SummaryReportType.GroupByDay
            },
            token: _mcpJwtToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(toolResult);
        Assert.True(toolResult.IsError);
    }
}
