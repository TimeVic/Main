using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue.Default;

public class ProcessJiraSendingContextTest: BaseTest
{
    private readonly IQueueService _queueService;
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly TimeEntryEntity _timeEntry;
    private readonly JiraClientMock _jiraClient;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly TaskListEntity _taskList;
    private readonly TaskEntity _task;

    private readonly string _apiToken;
    private readonly string _userName;
    private readonly string _taskId;
    private readonly string? _url;

    public ProcessJiraSendingContextTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _jiraClient = Scope.Resolve<IJiraClient>() as JiraClientMock;
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        _userDao = Scope.Resolve<IUserDao>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        
        var configuration = Scope.Resolve<IConfiguration>();
        _apiToken = configuration.GetValue<string>("Integration:Jira:ApiToken");
        _userName = configuration.GetValue<string>("Integration:Jira:UserName");
        // _taskId = configuration.GetValue<string>("Integration:Jira:TaskId");
        _taskId = "SP-2315";
        _url = configuration.GetValue<string>("Integration:Jira:Url");
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(_taskList).Result;
        _timeEntry = _timeEntrySeeder.CreateSeveralAsync(_workspace, _user).Result.First();
        _task.ExternalTaskId = _taskId;
        _timeEntry.Task = _task;
        
        var settings = _workspaceSettingsDao.SetJiraAsync(
            _user,
            _workspace,
            _url,
            _apiToken,
            _userName,
            true
        ).Result;
        settings.IsActive = true;

        FlushDbChanges().Wait();
        _queueDao.CompleteAllPending().Wait();
        Assert.NotNull(_jiraClient);
        _jiraClient.Reset();
    }

    [Fact]
    public async Task ShouldProcessJiraTimeEntryRequest()
    {
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.JiraId);

        await _queueService.PushExternalClientAsync(testContext);
        FlushDbChanges().Wait();
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _jiraClient.SentTimeEntries.Count);

        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.JiraId);
    }
    
    [Fact]
    public async Task ShouldDoNothingIfUserDoesNotHaveJiraConfiguration()
    {
        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var settings = _workspace.GetJiraSettings(_user.Id); 
        Assert.NotNull(settings);
        settings.IsActive = false;
        
        var timeEntryWithAnotherUser = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user)).First();
        timeEntryWithAnotherUser.TaskId = _taskId;
        await FlushDbChanges();
        
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = timeEntryWithAnotherUser.Id
        };
        Assert.Null(timeEntryWithAnotherUser.JiraId);

        await _queueService.PushExternalClientAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(0, _jiraClient.SentTimeEntries.Count);

        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(timeEntryWithAnotherUser);
        Assert.Null(timeEntryWithAnotherUser.JiraId);
    }
    
    [Fact]
    public async Task ShouldFillTimeEntryFromTaskDetails()
    {
        _timeEntry.Description = "";
        await FlushDbChanges();
        
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.JiraId);

        await _queueService.PushExternalClientAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _jiraClient.SentTimeEntries.Count);

        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.JiraId);
        Assert.NotEmpty(_timeEntry.Description);
    }
}
