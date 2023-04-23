using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue.Default;

public class ProcessRedmineSendingContextTest : BaseTest
{
    private readonly IQueueService _queueService;
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly TimeEntryEntity _timeEntry;
    private readonly RedmineClientMock _redmineClient;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITaskListSeeder _taskListSeeder;

    private readonly string _apiKey;
    private readonly long _userId;
    private readonly string _taskId;
    private readonly string? _redmineUrl;
    private readonly long _activityId;
    private readonly ProjectEntity _project;
    private readonly TaskListEntity _taskList;
    private readonly TaskEntity _task;

    public ProcessRedmineSendingContextTest() : base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _redmineClient = Scope.Resolve<IRedmineClient>() as RedmineClientMock;
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _apiKey = configuration.GetValue<string>("Integration:Redmine:ApiKey");
        _userId = configuration.GetValue<long>("Integration:Redmine:UserId");
        _taskId = configuration.GetValue<string>("Integration:Redmine:TaskId");
        _activityId = configuration.GetValue<long>("Integration:Redmine:ActivityId");
        _redmineUrl = configuration.GetValue<string>("Integration:Redmine:Url");

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(_taskList).Result;
        
        _timeEntry = _timeEntrySeeder.CreateSeveralAsync(_workspace, _user).Result.First();
        _task.ExternalTaskId = _taskId;
        _timeEntry.Task = _task;

        var settings = _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _workspace,
            _redmineUrl,
            _apiKey,
            _userId,
            _activityId
        ).Result;
        settings.IsActive = true;

        CommitDbChanges().Wait();
        _queueDao.CompleteAllPending().Wait();
        _redmineClient.Reset();
    }

    [Fact]
    public async Task ShouldProcessTimeEntryRequest()
    {
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.RedmineId);

        await _queueService.PushExternalClientAsync(testContext);
        CommitDbChanges().Wait();

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _redmineClient.SentTimeEntries.Count);

        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.RedmineId);
    }

    [Fact]
    public async Task ShouldDoNothingIfUserDoesNotHaveConfiguration()
    {
        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var settings = _workspace.GetRedmineSettings(_user.Id);
        settings.IsActive = false;

        var timeEntryWithAnotherUser = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user)).First();
        timeEntryWithAnotherUser.TaskId = _taskId;
        await CommitDbChanges();

        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = timeEntryWithAnotherUser.Id
        };
        Assert.Null(timeEntryWithAnotherUser.RedmineId);

        await _queueService.PushExternalClientAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(0, _redmineClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(timeEntryWithAnotherUser);
        Assert.Null(timeEntryWithAnotherUser.RedmineId);
    }
}
