using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Jira;

public partial class SendNewTimeEntityTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IJiraClient _client;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
       
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;

    private readonly string _apiToken;
    private readonly string _userName;
    private readonly string _taskId;
    private readonly string? _url;

    public SendNewTimeEntityTest() : base(false)
    {
        _client = Scope.Resolve<IJiraClient>();
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
        _apiToken = configuration.GetValue<string>("Integration:Jira:ApiToken");
        _userName = configuration.GetValue<string>("Integration:Jira:UserName");
        _taskId = configuration.GetValue<string>("Integration:Jira:TaskId");
        _url = configuration.GetValue<string>("Integration:Jira:Url");

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();

        _workspaceSettingsDao.SetJiraAsync(
            _user,
            _workspace,
            _url,
            _apiToken,
            _userName,
            true
        ).Wait();
    }

    // [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        
        var date = DateTime.UtcNow.Date.ToDateOnly();
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date.ToDateOnly(),
            DateTime.UtcNow.TimeOfDay,
            true,
            description: fakeTimeEntry.Description
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = _taskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.TimeOfDay, date);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _client.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.NotEmpty(actualResponse.Id);
        Assert.Equal(fakeTimeEntry.Description, actualResponse.Comment);

        activeEntry.JiraId = long.Parse(actualResponse.Id);
        var isDeleted = await _client.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    // [Fact]
    public async Task ShouldReceiveErrorIfTaskNotFound()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var date = DateTime.UtcNow.Date.ToDateOnly();
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            date,
            TimeSpan.FromMinutes(1),
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = "fake";
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);

        var actualResponse = await _client.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.True(actualResponse.IsError);
    }

    // [Fact]
    public async Task ShouldUpdateExistsTimeEntry()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var fakeTimeEntry = _timeEntryFactory.Generate();
        
        var date = DateTime.UtcNow.Date.ToDateOnly();
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            date,
            DateTime.UtcNow.TimeOfDay,
            true
        );
        activeEntry.Task = await _taskSeeder.CreateAsync(taskList: taskList);
        activeEntry.Task.ExternalTaskId = _taskId;
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, DateTime.UtcNow.TimeOfDay, date);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var creatingResponse = await _client.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(creatingResponse);
        Assert.False(creatingResponse.IsError);
        activeEntry.JiraId = long.Parse(creatingResponse.Id);
        await DbSessionProvider.CurrentSession.SaveAsync(activeEntry);
        await FlushDbChanges();
    
        activeEntry = await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
        {
            Id = activeEntry.Id,
            StartTime = DateTime.UtcNow.TimeOfDay,
            EndTime = DateTime.UtcNow.AddMilliseconds(5).TimeOfDay,
            Description = fakeTimeEntry.Description,
        });
        var actualResponse = await _client.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.Equal(fakeTimeEntry.Description, actualResponse.Comment);
        
        var isDeleted = await _client.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }

    // [Fact]
    public async Task ShouldGetTaskDetails()
    {
        var date = DateTime.UtcNow.Date.ToDateOnly();
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
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var getTaskResponse = await _client.GetTaskAsync(
            activeEntry,
            _taskId
        );
        Assert.NotNull(getTaskResponse);
        Assert.NotEmpty(getTaskResponse.Fields.Summary);
    }
}
