using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public partial class SendNewTimeEntityTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IClickUpClient _сlickUpClient;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    
    private readonly string _securityKey;
    private readonly string _teamId;
    private readonly string _externalTaskId;
    private readonly string _externalTaskIdForCreation;
    
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;

    public SendNewTimeEntityTest() : base(false)
    {
        _сlickUpClient = Scope.Resolve<IClickUpClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _timeEntryFactory = Scope.Resolve<IDataFactory<TimeEntryEntity>>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _externalTaskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        _externalTaskIdForCreation = configuration.GetValue<string>("Integration:ClickUp:TaskIdForCreation");

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

    // TODO: Restore: Your plan is limited to {{limit}} usages of Advanced Time Tracking, {{usage}} usage.
    // [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true,
            fakeTimeEntry.Description
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = _externalTaskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.AddMinutes(1));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.NotEmpty(actualResponse.Id);
        Assert.Equal(fakeTimeEntry.Description, actualResponse.Comment);

        activeEntry.ClickUpId = actualResponse.Id;
        var isDeleted = await _сlickUpClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    // [Fact]
    public async Task ShouldReceiveErrorIfTaskNotFound()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = "fake";
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, startTime.AddMinutes(2));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);

        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.True(actualResponse.IsError);
    }

    // TODO: Restore: Your plan is limited to {{limit}} usages of Advanced Time Tracking, {{usage}} usage.
    // [Fact]
    public async Task ShouldUpdateExistsTimeEntry()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var fakeTimeEntry = _timeEntryFactory.Generate();
        
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = _externalTaskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, startTime.AddMinutes(1));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var creatingResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(creatingResponse);
        Assert.False(creatingResponse.IsError);
        activeEntry.ClickUpId = creatingResponse.Id;
        await DbSessionProvider.CurrentSession.SaveAsync(activeEntry);
        await FlushDbChanges();
    
        activeEntry = await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Id = activeEntry.Id,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMilliseconds(5),
            Description = fakeTimeEntry.Description,
        });
        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.Equal(fakeTimeEntry.Description, actualResponse.Comment);
        
        var isDeleted = await _сlickUpClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    // [Fact]
    public async Task ShouldGetTaskDetails()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true
        );
        activeEntry.TaskId = _externalTaskId;
        
        // Description should be empty
        activeEntry.Description = "";
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, startTime.AddMinutes(1));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var getTaskResponse = await _сlickUpClient.GetTaskAsync(
            activeEntry,
            _externalTaskId
        );
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
