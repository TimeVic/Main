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

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Report.UserPaymentReportTest;

public class ForUserTest : BaseTest
{
    private readonly string _url = $"/{ApiUrl.ReportUserPayment}";
    private readonly string _memberToken;
    private readonly UserEntity _member;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _workspace;
    private readonly ProjectEntity _project;
    private readonly ClientEntity _client;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IClientPaymentDao _clientPaymentDao;
    private readonly IMemberPaymentDao _memberPaymentDao;

    public ForUserTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _clientPaymentDao = ServiceProvider.GetRequiredService<IClientPaymentDao>();
        _memberPaymentDao = ServiceProvider.GetRequiredService<IMemberPaymentDao>();
        var projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        var workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();

        var (_, owner, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _owner = owner;
        (_memberToken, _member, _) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace = workspace;
        _workspace.Mode = WorkspaceMode.Solo;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();
        FlushDbChanges().Wait();
        _project = projectSeeder.CreateAsync(_workspace).Result;
        _client = _project.Client!;

        workspaceAccessService.ShareAccessAsync(
            _workspace,
            _member,
            MembershipAccessType.User,
            new List<ProjectAccessModel> { new() { Project = _project } }
        ).Wait();

        var startTime = DateTime.UtcNow.StartOfDay().AddHours(9);
        _timeEntryDao.SetAsync(_member, _workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 100
        }, _project).Wait();
        _timeEntryDao.SetAsync(_owner, _workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(3),
            IsBillable = true,
            HourlyRate = 100
        }, _project).Wait();

        _clientPaymentDao.CreateAsync(_client, 75, startTime, _project.Id).Wait();
        _clientPaymentDao.CreateAsync(_client, 25, startTime).Wait();
    }

    [Fact]
    public async Task AnonymousCannotAccessReport()
    {
        var response = await PostRequestAsAnonymousAsync(_url, new UserPaymentReportRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserCanAccessReportForSharedWorkspace()
    {
        var response = await PostRequestAsync(_url, _memberToken, new UserPaymentReportRequest(), _workspace.Id);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UserCanAccessPersonalPayoutsInTeamWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Team;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();

        var response = await PostRequestAsync(_url, _memberToken, new UserPaymentReportRequest(), _workspace.Id);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ReportAggregatesCurrentUsersEarnedAndClientPayments()
    {
        var response = await PostRequestAsync(_url, _memberToken, new UserPaymentReportRequest(), _workspace.Id);
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<UserPaymentReportResponse>();
        Assert.NotNull(data);
        Assert.Equal(200, data.Totals.Earned);
        Assert.Equal(100, data.Totals.Received);
        Assert.Equal(100, data.Totals.Outstanding);
        Assert.False(data.IsPaymentsFromMembers);

        var client = Assert.Single(data.Clients);
        Assert.Equal(_client.Id, client.Id);
        Assert.Equal(TimeSpan.FromHours(2), client.Duration);
        Assert.Equal(200, client.Earned);
        Assert.Equal(75, client.ProjectPayments);
        Assert.Equal(25, client.GeneralPayments);
        Assert.Equal(100, client.Received);
        Assert.Equal(100, client.Outstanding);

        var project = Assert.Single(client.Projects);
        Assert.Equal(_project.Id, project.Id);
        Assert.Equal(TimeSpan.FromHours(2), project.Duration);
        Assert.Equal(200, project.Earned);
    }

    [Fact]
    public async Task TeamWorkspaceReportAggregatesOnlyCurrentUsersMemberPayments()
    {
        _workspace.Mode = WorkspaceMode.Team;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();
        await _memberPaymentDao.CreateAsync(_workspace, _member, _project, 75, DateTime.UtcNow);
        await _memberPaymentDao.CreateAsync(_workspace, _owner, _project, 250, DateTime.UtcNow);
        await FlushDbChanges();

        var response = await PostRequestAsync(
            _url,
            _memberToken,
            new UserPaymentReportRequest { EndDate = DateTime.UtcNow },
            _workspace.Id
        );
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<UserPaymentReportResponse>();
        Assert.True(data.IsPaymentsFromMembers);
        Assert.Equal(200, data.Totals.Earned);
        Assert.Equal(75, data.Totals.Received);
        Assert.Equal(125, data.Totals.Outstanding);

        var client = Assert.Single(data.Clients);
        Assert.Equal(75, client.ProjectPayments);
        Assert.Equal(0, client.GeneralPayments);
        Assert.Equal(75, client.Received);
    }

    [Fact]
    public async Task SoloWorkspaceReportExcludesWorkAndClientPaymentsAfterEndDate()
    {
        var futureStartTime = DateTime.UtcNow.AddDays(1).StartOfDay().AddHours(9);
        await _timeEntryDao.SetAsync(_member, _workspace, new TimeEntryCreationDto
        {
            StartTime = futureStartTime,
            EndTime = futureStartTime.AddHours(1),
            IsBillable = true,
            HourlyRate = 100
        }, _project);
        await _clientPaymentDao.CreateAsync(_client, 50, futureStartTime, _project.Id);
        await FlushDbChanges();

        var response = await PostRequestAsync(
            _url,
            _memberToken,
            new UserPaymentReportRequest { EndDate = DateTime.UtcNow },
            _workspace.Id
        );
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<UserPaymentReportResponse>();
        Assert.Equal(200, data.Totals.Earned);
        Assert.Equal(100, data.Totals.Received);
        Assert.Equal(100, data.Totals.Outstanding);
    }
}
