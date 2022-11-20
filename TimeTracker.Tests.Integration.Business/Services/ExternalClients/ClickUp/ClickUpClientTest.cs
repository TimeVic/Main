using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public class SendNewTimeEntityTest : BaseTest
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
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;

    // TODO: Revert test
    public SendNewTimeEntityTest() : base(false)
    {
        _сlickUpClient = Scope.Resolve<IClickUpClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _taskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();

        _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            _securityKey,
            _teamId,
            true,
            true
        ).Wait();
    }

    [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.TimeOfDay,
            true
        );
        activeEntry.TaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.TimeOfDay, date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.NotEmpty(actualResponse.Id);

        activeEntry.ClickUpId = actualResponse.Id;
        var isDeleted = await _сlickUpClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    [Fact]
    public async Task ShouldReceiveErrorIfTaskNotFound()
    {
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            date,
            TimeSpan.FromMinutes(1),
            true
        );
        activeEntry.TaskId = "fake";
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);

        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.True(actualResponse.IsError);
    }

    [Fact]
    public async Task ShouldUpdateExistsTimeEntry()
    {
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            date,
            DateTime.UtcNow.TimeOfDay,
            true
        );
        activeEntry.TaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.TimeOfDay, date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var creatingResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.False(creatingResponse.IsError);
        activeEntry.ClickUpId = creatingResponse.Id;
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
        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.False(actualResponse.IsError);
        
        var isDeleted = await _сlickUpClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    [Fact]
    public async Task ShouldGetTaskDetails()
    {
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            date,
            TimeSpan.FromMinutes(1),
            true
        );
        activeEntry.TaskId = _taskId;
        
        // Description should be empty
        activeEntry.Description = "";
        await CommitDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var getTaskResponse = await _сlickUpClient.GetTaskAsync(activeEntry);
        Assert.NotEmpty(getTaskResponse.Value.Name);
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
