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
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client!;

        _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.StartOfDay().AddHours(9),
            EndTime = DateTime.UtcNow.StartOfDay().AddHours(11),
            IsBillable = true,
            HourlyRate = 100
        }, _project).Wait();
    }

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
    public async Task SoloWorkspaceIsNotTeamWorkspace()
    {
        var response = await PostRequestAsync(_url, _ownerToken, new WorkspaceFinancialSummaryReportRequest
        {
        });
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<WorkspaceFinancialSummaryReportResponse>();
        Assert.NotNull(data);
        Assert.False(data.IsTeamWorkspace);
    }

    [Fact]
    public async Task TeamWorkspaceFlagSetWhenMultipleMembers()
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
