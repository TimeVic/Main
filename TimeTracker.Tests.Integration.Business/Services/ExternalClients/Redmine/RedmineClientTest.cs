using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Redmine;

public partial class RedmineClientTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly IRedmineClient _redmineClient;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;
    
    private readonly string _apiKey = string.Empty;
    private readonly long _userId;
    private readonly string _taskId = string.Empty;
    private readonly string? _redmineUrl;
    private readonly long _activityId;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;
    private readonly TaskListEntity _taskList;

    public RedmineClientTest() : base(false)
    {
        _redmineClient = Scope.Resolve<IRedmineClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userDao = Scope.Resolve<IUserDao>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();

        var configuration = Scope.Resolve<IConfiguration>();
        _apiKey = configuration.GetValue<string>("Integration:Redmine:ApiKey")!;
        _userId = configuration.GetValue<long>("Integration:Redmine:UserId");
        _taskId = configuration.GetValue<string>("Integration:Redmine:TaskId")!;
        _activityId = configuration.GetValue<long>("Integration:Redmine:ActivityId");
        _redmineUrl = configuration.GetValue<string>("Integration:Redmine:Url");

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();

        _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _workspace,
            _redmineUrl,
            _apiKey,
            _userId,
            _activityId
        ).Wait();
        
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
    }

    // [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var expectedDescription = "Test description";
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.AddMinutes(1),
            true,
            description: expectedDescription
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: _taskList);
        activeEntry.Task.ExternalTaskId = _taskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.AddMinutes(2));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.NotEmpty(actualResponse.Id);
        Assert.Equal(expectedDescription, actualResponse.Comment);

        activeEntry.RedmineId = actualResponse.Id;
        var isDeleted = await _redmineClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    // [Fact]
    public async Task ShouldReceiveErrorIfTaskNotFound()
    {
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.AddMinutes(1),
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: _taskList);
        activeEntry.Task.ExternalTaskId = "fake";
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.AddMinutes(2));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.True(actualResponse.IsError);
    }

    // [Fact]
    public async Task ShouldUpdateExistsTimeEntry()
    {
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: _taskList);
        activeEntry.Task.ExternalTaskId = _taskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, startTime.AddMinutes(2));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var creatingResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(creatingResponse);
        Assert.False(creatingResponse.IsError);
        activeEntry.RedmineId = creatingResponse.Id;
        await DbSessionProvider.CurrentSession.SaveAsync(activeEntry);
        await FlushDbChanges();
    
        activeEntry = await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Id = activeEntry.Id,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMilliseconds(5),
            Description = "Test"
        });
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        
        var isDeleted = await _redmineClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }
}
