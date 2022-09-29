using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Integrations;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public class SendNewTimeEntityTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IClickUpClient _сlickUpClient;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly string _securityKey;

    private readonly string _teamId;
    private readonly string _taskId;
    
    public SendNewTimeEntityTest(): base(false)
    {
        _сlickUpClient = Scope.Resolve<IClickUpClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        
        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _taskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
        
        _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            _securityKey,
            _teamId,
            true
        ).Wait();
    }

    [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        activeEntry.TaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        
        var actualResponse = await _сlickUpClient.SendTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.Value.IsError);
        Assert.True(actualResponse.Value.Id > 0);
    }
    
    [Fact]
    public async Task ShouldReceiveErrorIfTaskNotFound()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        activeEntry.TaskId = "fake";
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        
        var actualResponse = await _сlickUpClient.SendTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.True(actualResponse.Value.IsError);
    }
    
    [Fact]
    public async Task ShouldUpdateExistsTimeEntry()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(_user, _workspace, true);
        activeEntry.TaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
        
        var creatingResponse = await _сlickUpClient.SendTimeEntryAsync(activeEntry);
        Assert.False(creatingResponse.Value.IsError);
        activeEntry.ClickUpId = creatingResponse.Value.Id;
        await DbSessionProvider.CurrentSession.SaveAsync(activeEntry);
        await CommitDbChanges();
        
        activeEntry = await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Id = activeEntry.Id,
            StartTime = DateTime.UtcNow.TimeOfDay,
            EndTime = DateTime.UtcNow.AddMilliseconds(5).TimeOfDay,
            Description = "Test",
            TaskId = activeEntry.TaskId
        });
        var actualResponse = await _сlickUpClient.SendTimeEntryAsync(activeEntry);
        Assert.False(actualResponse.Value.IsError);
    }

    [Fact]
    public void ShouldRemoveFirstSymbolFromId()
    {
        var taskId = " #abd123 ";
        var actualTaskId = ClickUpClient.CleanUpTaskId(taskId, false);
        Assert.Equal("abd123", actualTaskId);
    }
    
    [Fact]
    public void ShouldNotRemoveFirstSymbolFromId()
    {
        var expectedTaskId = " #abd123 ";
        var actualTaskId = ClickUpClient.CleanUpTaskId(expectedTaskId, true);
        Assert.Equal(expectedTaskId.Trim(), actualTaskId);
    }
}
