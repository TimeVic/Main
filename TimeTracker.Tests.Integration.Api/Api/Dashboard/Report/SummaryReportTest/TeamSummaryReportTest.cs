using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Report.SummaryReportTest;

public class TeamSummaryReportTest : BaseTest
{
    private readonly string _teamUrl = $"/{ApiUrl.ReportSummaryTeam}";
    private readonly string _ownerToken;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _workspace;
    private readonly ProjectEntity _project;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public TeamSummaryReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        var projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_ownerToken, _owner, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();
        FlushDbChanges().Wait();
        _project = projectSeeder.CreateAsync(_workspace).Result;
    }

    [Fact]
    public async Task OwnerCanViewTeamFinancialsAndMemberPerformance()
    {
        await AddOwnerEntryAsync();
        var (_, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel> { new() { Project = _project, HourlyRate = 40 } }
        );
        var startTime = DateTime.UtcNow.StartOfDay().AddHours(12);
        await _timeEntryDao.SetAsync(member, _workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 100
        }, _project);

        var response = await PostRequestAsync(_teamUrl, _ownerToken, CreatePeriodRequest());
        response.EnsureSuccessStatusCode();

        var report = await response.GetJsonDataAsync<TeamSummaryReportResponse>();
        Assert.Equal(TimeSpan.FromHours(4), report.Totals.Duration);
        Assert.Equal(300, report.Totals.ClientBillable);
        Assert.Equal(80, report.Totals.TeamLaborCost);
        Assert.Equal(220, report.Totals.GrossProfit);
        Assert.Equal(2, report.Members.Count);
        var memberReport = Assert.Single(report.Members, item => item.Email == member.Email);
        Assert.Equal(80, memberReport.TeamLaborCost);
        Assert.Equal(120, memberReport.GrossProfit);
    }

    [Fact]
    public async Task RegularMemberCannotViewTeamSummary()
    {
        var (memberToken, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel> { new() { Project = _project, HourlyRate = 40 } }
        );

        var response = await PostRequestAsync(_teamUrl, memberToken, CreatePeriodRequest(), _workspace.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task AddOwnerEntryAsync()
    {
        var startTime = DateTime.UtcNow.StartOfDay().AddHours(9);
        await _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 50
        }, _project);
    }

    private static TeamSummaryReportRequest CreatePeriodRequest()
    {
        return new TeamSummaryReportRequest
        {
            StartTime = DateTime.UtcNow.StartOfDay(),
            EndTime = DateTime.UtcNow.EndOfDay()
        };
    }
}
