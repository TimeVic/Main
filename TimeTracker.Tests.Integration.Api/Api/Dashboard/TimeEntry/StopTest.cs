using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public class StopTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/stop";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private new readonly IQueueDao _queueDao;
    private readonly IQueueService _queueService;
    private readonly ITaskSeeder _taskSeeder;

    public StopTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new StopRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldStopActive()
    {
        var expectedEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow.AddMinutes(-1)
        );
        
        var stopRequestedAt = DateTime.UtcNow;
        var response = await PostRequestAsync(Url, _jwtToken, new StopRequest());
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Equal(expectedEntry.Id, actualDto.Id);
        Assert.NotNull(actualDto.EndTime);
        Assert.InRange(actualDto.EndTime.Value, stopRequestedAt, DateTime.UtcNow);

        await DbSessionProvider.CurrentSession.RefreshAsync(_defaultWorkspace);
        Assert.False(await _workspaceDao.HasActiveTimeEntriesAsync(_defaultWorkspace));
        
        var processedCounter = await QueueProcess(QueueChannel.ExternalClient);
        Assert.True(processedCounter > 0);
    }
    
    [Fact]
    public async Task ShouldReturnNullIfNotActive()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new StopRequest());
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Equal(Guid.Empty, actualDto.Id);
        
        var processedCounter = await QueueProcess(QueueChannel.ExternalClient);
        Assert.True(processedCounter == 0);
    }

    [Fact]
    public async Task ShouldReturnLinkedTaskWithTrackedDuration()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        var startTime = DateTime.UtcNow.AddHours(-2);
        var expectedEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            startTime,
            internalTask: task
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new StopRequest());
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Equal(expectedEntry.Id, actualDto.Id);
        Assert.NotNull(actualDto.Task);
        Assert.Equal(task.Id, actualDto.Task.Id);
        Assert.InRange(
            actualDto.Task.TrackedDuration,
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(1))
        );
    }
}
