using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

public class StopTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/stop";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IQueueDao _queueDao;
    private readonly IQueueService _queueService;

    public StopTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
        
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new StopRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndTime = TimeSpan.FromHours(1)
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldStopActive()
    {
        var expectedEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.Now,
            TimeSpan.FromSeconds(1)
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new StopRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndTime = TimeSpan.FromHours(1)
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_defaultWorkspace);
        Assert.False(await _workspaceDao.HasActiveTimeEntriesAsync(_defaultWorkspace));
        
        var processedCounter = await _queueService.ProcessAsync(QueueChannel.Default);
        Assert.True(processedCounter > 0);
    }
    
    [Fact]
    public async Task ShouldReturnNullIfNotActive()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new StopRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            EndTime = TimeSpan.FromHours(1)
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Null(actualDto);
        
        var processedCounter = await _queueService.ProcessAsync(QueueChannel.Default);
        Assert.True(processedCounter == 0);
    }
}
