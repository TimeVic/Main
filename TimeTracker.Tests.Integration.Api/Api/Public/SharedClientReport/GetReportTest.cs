using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
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

    public GetReportTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        var projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        var timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        var clientPaymentDao = ServiceProvider.GetRequiredService<IClientPaymentDao>();
        var sharedReportDao = ServiceProvider.GetRequiredService<ISharedClientReportDao>();
        var taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        var taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        var (_, user, workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        var project = projectSeeder.CreateAsync(workspace).Result;
        var client = project.Client;
        var startTime = DateTime.UtcNow.StartOfDay().AddHours(9);

        timeEntryDao.SetAsync(user, workspace, new TimeEntryCreationDto
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            IsBillable = true,
            HourlyRate = 100
        }, project).Wait();
        clientPaymentDao.CreateAsync(client, 50, startTime, project.Id, "Initial payment").Wait();

        var taskList = taskListSeeder.CreateAsync(project).Result;
        var trackedTask = taskSeeder.CreateAsync(taskList, user).Result;
        var untrackedTask = taskSeeder.CreateAsync(taskList, user).Result;
        var taskEntry = timeEntryDao.StartNewAsync(
            user,
            workspace,
            startTime.AddHours(3),
            isBillable: true,
            projectId: project.Id,
            hourlyRate: 100,
            internalTask: trackedTask
        ).Result;
        taskEntry.EndTime = startTime.AddHours(4);

        var activeReport = sharedReportDao.CreateAsync(client, "active-shared-client-report-token").Result;
        activeReport.IsActive = true;
        sharedReportDao.SaveAsync(activeReport).Wait();
        var inactiveClient = projectSeeder.CreateAsync(workspace).Result.Client;
        var inactiveReport = sharedReportDao.CreateAsync(inactiveClient, "inactive-shared-client-report-token").Result;

        _activeUrl = $"/{ApiUrl.PublicSharedClientReport}/{activeReport.Token}";
        _inactiveUrl = $"/{ApiUrl.PublicSharedClientReport}/{inactiveReport.Token}";
        _activeToken = activeReport.Token;
        _trackedTaskId = trackedTask.Id;
        _untrackedTaskId = untrackedTask.Id;
    }

    [Fact]
    public async Task AnonymousUserCanReadOnlyActiveClientCommercialReport()
    {
        var response = await GetRequestAsAnonymousAsync(_activeUrl);
        response.EnsureSuccessStatusCode();

        var report = await response.GetJsonDataAsync<GetSharedClientReportResponse>();
        var project = Assert.Single(report.Projects);
        var payment = Assert.Single(report.Payments);
        Assert.Equal(TimeSpan.FromHours(3), project.Duration);
        Assert.Equal(300, project.Earned);
        Assert.Equal(300, report.Totals.Earned);
        Assert.Equal(50, report.Totals.Received);
        Assert.Equal(250, report.Totals.Outstanding);
        Assert.Equal("Initial payment", payment.Description);
        Assert.True(report.IsShowTasks);
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
        var response = await GetRequestAsAnonymousAsync($"/{ApiUrl.PublicSharedClientReport}/{_activeToken}/get-tasks");
        response.EnsureSuccessStatusCode();

        var data = await response.GetJsonDataAsync<GetSharedClientReportTasksResponse>();
        var task = Assert.Single(data.Tasks);
        Assert.Equal(_trackedTaskId, task.Id);
        Assert.NotEqual(_untrackedTaskId, task.Id);
        Assert.Equal(TimeSpan.FromHours(1), task.Duration);
    }
}
