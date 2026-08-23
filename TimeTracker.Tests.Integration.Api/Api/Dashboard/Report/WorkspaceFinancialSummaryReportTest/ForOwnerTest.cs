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

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Report.WorkspaceFinancialSummaryReportTest;

public class ForOwnerTest : BaseTest
{
    private readonly string _url = $"/{ApiUrl.ReportWorkspaceFinancialSummary}";

    private readonly UserEntity _owner;
    private readonly string _ownerToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IClientPaymentSeeder _clientPaymentSeeder;
    private readonly IMemberPaymentSeeder _memberPaymentSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    private readonly ProjectEntity _project;
    private readonly ClientEntity _client;

    public ForOwnerTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _clientPaymentSeeder = ServiceProvider.GetRequiredService<IClientPaymentSeeder>();
        _memberPaymentSeeder = ServiceProvider.GetRequiredService<IMemberPaymentSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();

        (_ownerToken, _owner, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();
        FlushDbChanges().Wait();
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client!;

        var entry = _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
            StartTime = DateTime.UtcNow.StartOfDay().AddHours(9),
            EndTime = DateTime.UtcNow.StartOfDay().AddHours(11),
            IsBillable = true,
            HourlyRate = 100
        }, _project).Result;
        entry.Status = TimeEntryStatus.Approved;
    [Fact]
    public async Task AnonymousCanNotAccess()
    {
        var response = await PostRequestAsAnonymousAsync(_url, new WorkspaceFinancialSummaryReportRequest
        {
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OwnerCanAccessReport()
    {
        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data);
    }

    [Fact]
    public async Task ManagerCanAccessReport()
    {
        var (managerToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );

        var response = await PostRequestAsync(_url, managerToken, new WorkspaceFinancialSummaryReportRequest(), _workspace.Id);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RegularMemberCannotAccess()
    {
        var (memberToken, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );

        var response = await PostRequestAsync(_url, memberToken, new WorkspaceFinancialSummaryReportRequest
        {
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ClientEarnedCalculatedCorrectly()
    {
        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.True(data.Totals.ClientEarned > 0);
    }

    [Fact]
    public async Task ClientOutstandingEqualsEarnedMinusReceived()
    {
        await _clientPaymentSeeder.CreateSeveralAsync(_client, _project, 1);

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.Equal(
            data.Totals.ClientEarned - data.Totals.ClientReceived,
            data.Totals.ClientOutstanding
        );
    }

    [Fact]
    public async Task SoloWorkspaceCannotAccessTeamFinancialReport()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TeamWorkspaceFlagIsSet()
    {
        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data);
        Assert.True(data.IsTeamWorkspace);
    }

    [Fact]
    public async Task EstimatedMarginEqualsClientEarnedMinusTeamCost()
    {
        var (_, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.Equal(
            data.Totals.ClientEarned - data.Totals.TeamCost,
            data.Totals.EstimatedMargin
        );
    }

    [Fact]
    public async Task ReportIgnoresTimeEntriesWithoutBillableRate()
    {
        _project.DefaultHourlyRate = 1000;

        var entry1 = await _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.StartOfDay().AddHours(12),
            IsBillable = false,
            HourlyRate = 100
        }, _project);
        entry1.Status = TimeEntryStatus.Approved;

        var entry2 = await _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
            StartTime = DateTime.UtcNow.StartOfDay().AddHours(14),
            EndTime = DateTime.UtcNow.StartOfDay().AddHours(15),
            HourlyRate = null
        }, _project);
        entry2.Status = TimeEntryStatus.Approved;

        var entry3 = await _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
        {
            EndTime = DateTime.UtcNow.StartOfDay().AddHours(17),
            IsBillable = true,
        }, _project);
        entry3.Status = TimeEntryStatus.Approved;
        await FlushDbChanges();

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.Equal(260, data.Totals.ClientEarned);
        Assert.Equal(0, data.Totals.TeamCost);
        Assert.Equal(260, data.Totals.EstimatedMargin);

        var projectProfitability = Assert.Single(data.ProjectProfitability);
        Assert.Equal(260, projectProfitability.ClientEarned);
        Assert.Equal(0, projectProfitability.TeamCost);
        Assert.Equal(260, projectProfitability.EstimatedMargin);
    }

    [Fact]
    public async Task TeamCostUsesMemberInternalProjectRate()
    {
        var (_, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel> { new() { Project = _project, HourlyRate = 40 } }
        );
        var startTime = DateTime.UtcNow.StartOfDay().AddHours(12);
        var memberEntry = await _timeEntryDao.SetAsync(member, _workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 100
        memberEntry.Status = TimeEntryStatus.Approved;
        await FlushDbChanges();

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest());
        response.EnsureSuccessStatusCode();

        Assert.Equal(400, data.Totals.ClientEarned);
        Assert.Equal(80, data.Totals.TeamCost);
        Assert.Equal(320, data.Totals.EstimatedMargin);
        Assert.Equal(80, data.Totals.MarginPercent);
        Assert.Equal(80, Assert.Single(data.MemberBalances, item => item.User.Id == member.Id).Cost);
        var projectProfitability = Assert.Single(data.ProjectProfitability);
        Assert.Equal(80, projectProfitability.TeamCost);
        Assert.Equal(80, projectProfitability.MarginPercent);
    }

    [Fact]
    public async Task RealizedMarginEqualsClientReceivedMinusMemberPaidOut()
    {
        var (_, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );
        await _clientPaymentSeeder.CreateSeveralAsync(_client, _project, 1);
        await _memberPaymentSeeder.CreateSeveralAsync(_workspace, _owner, 1);

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.Equal(
            data.Totals.ClientReceived - data.Totals.MemberPaidOut,
            data.Totals.RealizedMargin
        );
    }

    [Fact]
    public async Task MemberOwedEqualsTeamCostMinusPaidOut()
    {
        var (_, member, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            member,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );

        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data?.Totals);
        Assert.Equal(
            data.Totals.TeamCost - data.Totals.MemberPaidOut,
            data.Totals.MemberOutstanding
        );
    }
}
