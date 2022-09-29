using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao.Integrations;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue.Default;

public class ProcessClickUpSendingContextTest: BaseTest
{
    private readonly IQueueService _queueService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly TimeEntryEntity _timeEntry;
    private readonly ClickUpClientMock _clickUpClient;
    private readonly string _securityKey;
    private readonly string _teamId;
    private readonly string _taskId;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;

    public ProcessClickUpSendingContextTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clickUpClient = Scope.Resolve<IClickUpClient>() as ClickUpClientMock;
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        
        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _taskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
        _timeEntry = _timeEntrySeeder.CreateSeveralAsync(_user).Result.First();
        _queueDao.CompleteAllPending().Wait();
        _clickUpClient.Reset();
        _timeEntry.TaskId = _taskId;
        
        _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            _securityKey,
            _teamId,
            true
        ).Wait();
        CommitDbChanges().Wait();
    }

    [Fact]
    public async Task ShouldProcessClickUpTimeEntryRequest()
    {
        var testContext = new IntegrationAppQueueItemContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.ClickUpId);

        await _queueService.PushDefaultAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Default);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _clickUpClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.ClickUpId);
    }
    
    [Fact]
    public async Task ShouldDoNothingIfUserDoesNotHaveClickUpConfiguration()
    {
        var timeEntryWithAnotherUser = (await _timeEntrySeeder.CreateSeveralAsync()).First();
        timeEntryWithAnotherUser.TaskId = _taskId;
        await CommitDbChanges();
        
        var testContext = new IntegrationAppQueueItemContext()
        {
            TimeEntryId = timeEntryWithAnotherUser.Id
        };
        Assert.Null(timeEntryWithAnotherUser.ClickUpId);

        await _queueService.PushDefaultAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Default);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(0, _clickUpClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(timeEntryWithAnotherUser);
        Assert.Null(timeEntryWithAnotherUser.ClickUpId);
    }
}
