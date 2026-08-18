using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.SharedClientReport;

public class GetReportTest : BaseTest
{
    private readonly string _activeUrl;
    private readonly string _inactiveUrl;
    private readonly string _activeToken;
    private readonly Guid _trackedTaskId;
    private readonly Guid _untrackedTaskId;
    private readonly Guid _secondTrackedTaskId;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly ProjectEntity _project;
    private readonly TaskListEntity _taskList;
    private readonly ClientEntity _client;
    private readonly DateTime _startTime;
    private readonly IMemberPaymentDao _memberPaymentDao;
    private readonly ITaskDao _taskDao;

    public GetReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        var projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        var timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        var clientPaymentDao = ServiceProvider.GetRequiredService<IClientPaymentDao>();
        _memberPaymentDao = ServiceProvider.GetRequiredService<IMemberPaymentDao>();
        _taskDao = ServiceProvider.GetRequiredService<ITaskDao>();
        var sharedReportDao = ServiceProvider.GetRequiredService<ISharedClientReportDao>();
        var taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        var taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _user = user;
        _workspace = workspace;
        _project = projectSeeder.CreateAsync(workspace).Result;
        _client = _project.Client;
        _startTime = DateTime.UtcNow.StartOfDay().AddHours(9);

        timeEntryDao.SetAsync(user, workspace, new TimeEntryCreationDto
        {
            StartTime = _startTime,
            EndTime = _startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 100
        }, _project).Wait();
        clientPaymentDao.CreateAsync(_client, 50, _startTime, _project.Id, "Initial payment").Wait();

        _taskList = taskListSeeder.CreateAsync(_project).Result;
        var trackedTask = taskSeeder.CreateAsync(_taskList, user).Result;
        var untrackedTask = taskSeeder.CreateAsync(_taskList, user).Result;
        var secondTrackedTask = taskSeeder.CreateAsync(_taskList, user).Result;
        var taskEntry = timeEntryDao.StartNewAsync(
            user,
            workspace,
            _startTime.AddHours(3),
            isBillable: true,
            projectId: _project.Id,
            hourlyRate: 100,
            internalTask: trackedTask
        ).Result;
        taskEntry.EndTime = _startTime.AddHours(4);
        var secondTaskEntry = timeEntryDao.StartNewAsync(
            user,
            workspace,
            _startTime.AddHours(4),
            isBillable: true,
            projectId: _project.Id,
            hourlyRate: 100,
            internalTask: secondTrackedTask
        ).Result;
        secondTaskEntry.EndTime = _startTime.AddHours(4.5);

        var activeReport = sharedReportDao.CreateAsync(_client, "active-shared-client-report-token").Result;
        activeReport.IsActive = true;
        sharedReportDao.SaveAsync(activeReport).Wait();
        var inactiveClient = projectSeeder.CreateAsync(workspace).Result.Client;
        var inactiveReport = sharedReportDao.CreateAsync(inactiveClient, "inactive-shared-client-report-token").Result;

        _activeUrl = $"/{ApiUrl.PublicSharedClientReport}/{activeReport.Token}";
        _inactiveUrl = $"/{ApiUrl.PublicSharedClientReport}/{inactiveReport.Token}";
        _activeToken = activeReport.Token;
        _trackedTaskId = trackedTask.Id;
        _untrackedTaskId = untrackedTask.Id;
        _secondTrackedTaskId = secondTrackedTask.Id;
    }

    [Fact]
    public async Task AnonymousUserCanReadOnlyActiveClientCommercialReport()
    {
        var response = await GetRequestAsAnonymousAsync(_activeUrl);
        response.EnsureSuccessStatusCode();

        var report = await response.GetJsonDataAsync<GetSharedClientReportResponse>();
        var project = Assert.Single(report.Projects);
        var payment = Assert.Single(report.Payments);
        Assert.Equal(TimeSpan.FromHours(3.5), project.Duration);
        Assert.Equal(350, project.Earned);
        Assert.Equal(350, report.Totals.Earned);
        Assert.Equal(50, report.Totals.Received);
        Assert.Equal(300, report.Totals.Outstanding);
        Assert.Equal("Initial payment", payment.Description);
        Assert.True(report.IsShowTasks);
    }

    [Fact]
    public async Task SoloWorkspaceSharedReportUsesOnlyClientPayments()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await _memberPaymentDao.CreateAsync(_workspace, _user, _project, 40, _startTime.AddDays(1), "Team payout");
        await FlushDbChanges();

        var response = await GetRequestAsAnonymousAsync(_activeUrl);
        response.EnsureSuccessStatusCode();

        var report = await response.GetJsonDataAsync<GetSharedClientReportResponse>();
        Assert.Equal(50, report.Totals.Received);
        Assert.Single(report.Payments);
        Assert.Equal("Initial payment", report.Payments.Single().Description);
    }

    [Fact]
    public async Task TeamWorkspaceSharedReportUsesClientAndMemberPayments()
    {
        _workspace.Mode = WorkspaceMode.Team;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await _memberPaymentDao.CreateAsync(_workspace, _user, _project, 40, _startTime.AddDays(1), "Team payout");
        await _memberPaymentDao.CreateAsync(_workspace, _user, _project, 15, _startTime.AddDays(2), "Second payout");
        await FlushDbChanges();

        var response = await GetRequestAsAnonymousAsync(_activeUrl);
        response.EnsureSuccessStatusCode();

        var report = await response.GetJsonDataAsync<GetSharedClientReportResponse>();
        Assert.Equal(105, report.Totals.Received);
        Assert.Equal(3, report.Payments.Count);
        Assert.Contains(report.Payments, item => item.Description == "Initial payment");
        Assert.Contains(report.Payments, item => item.Description == "Team payout");
        Assert.Contains(report.Payments, item => item.Description == "Second payout");
        Assert.Equal("Second payout", report.Payments.First().Description);
    }

    [Fact]
    public async Task AnonymousUserGetsCultureFromAcceptLanguageHeader()
    {
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "uk-UA,uk;q=0.9,en;q=0.8");
        try
        {
            var response = await GetRequestAsAnonymousAsync(_activeUrl);
            response.EnsureSuccessStatusCode();

            var report = await response.GetJsonDataAsync<GetSharedClientReportResponse>();
            Assert.Equal("uk-UA", report.CultureCode);
        }
        finally
        {
            HttpClient.DefaultRequestHeaders.Remove("Accept-Language");
        }
    }

    [Fact]
    public async Task InactiveClientReportReturnsNotFound()
    {
        var response = await GetRequestAsAnonymousAsync(_inactiveUrl);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ActiveClientReportReturnsOnlyTasksWithTrackedTime()
    {
        var response = await GetRequestAsAnonymousAsync(
            $"/{ApiUrl.PublicSharedClientReport}/{_activeToken}/get-tasks",
            new Dictionary<string, string>
            {
                ["projectId"] = _project.Id.ToString(),
                ["page"] = "1"
            }
        );
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<GetSharedClientReportTasksResponse>();
        Assert.Equal(2, data.Tasks.Count);
        Assert.Contains(data.Tasks, item => item.Id == _trackedTaskId && item.Duration == TimeSpan.FromHours(1));
        Assert.Contains(data.Tasks, item => item.Id == _secondTrackedTaskId && item.Duration == TimeSpan.FromMinutes(30));
        Assert.DoesNotContain(data.Tasks, item => item.Id == _untrackedTaskId);
        Assert.False(data.IsHasMore);
    }

    [Fact]
    public async Task ActiveClientReportTasksArePaginatedByProject()
    {
        for (int index = 0; index < 35; index++)
        {
            var task = await _taskDao.AddTaskAsync(
                _taskList,
                _user,
                $"Task {index + 1}"
            );
            var taskEntry = await ServiceProvider.GetRequiredService<ITimeEntryDao>().StartNewAsync(
                _user,
                _workspace,
                _startTime.AddDays(2).AddMinutes(index * 10),
                isBillable: true,
                projectId: _project.Id,
                hourlyRate: 100,
                internalTask: task
            );
            taskEntry.EndTime = taskEntry.StartTime!.AddMinutes(5);
        }
        await FlushDbChanges();

        var firstPageResponse = await GetRequestAsAnonymousAsync(
            $"/{ApiUrl.PublicSharedClientReport}/{_activeToken}/get-tasks",
            new Dictionary<string, string>
            {
                ["projectId"] = _project.Id.ToString(),
                ["page"] = "1"
            }
        );
        firstPageResponse.EnsureSuccessStatusCode();
        var firstPage = await firstPageResponse.GetJsonDataAsync<GetSharedClientReportTasksResponse>();

        var secondPageResponse = await GetRequestAsAnonymousAsync(
            $"/{ApiUrl.PublicSharedClientReport}/{_activeToken}/get-tasks",
            new Dictionary<string, string>
            {
                ["projectId"] = _project.Id.ToString(),
                ["page"] = "2"
            }
        );
        secondPageResponse.EnsureSuccessStatusCode();
        var secondPage = await secondPageResponse.GetJsonDataAsync<GetSharedClientReportTasksResponse>();

        Assert.Equal(30, firstPage.Tasks.Count);
        Assert.True(firstPage.IsHasMore);
        Assert.Equal(37, firstPage.TotalCount);
        Assert.Equal(7, secondPage.Tasks.Count);
        Assert.False(secondPage.IsHasMore);
        Assert.All(firstPage.Tasks.Concat(secondPage.Tasks), item => Assert.Equal(_project.Id, item.ProjectId));
    }
}
