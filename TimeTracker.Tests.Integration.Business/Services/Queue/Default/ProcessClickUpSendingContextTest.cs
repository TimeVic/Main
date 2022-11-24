using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue.Default;

public class ProcessClickUpSendingContextTest: BaseTest
{
    private readonly IQueueService _queueService;
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
    private readonly IUserDao _userDao;

    public ProcessClickUpSendingContextTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clickUpClient = Scope.Resolve<IClickUpClient>() as ClickUpClientMock;
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        _userDao = Scope.Resolve<IUserDao>();
        
        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _taskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        _timeEntry = _timeEntrySeeder.CreateSeveralAsync(_workspace, _user).Result.First();
        _timeEntry.TaskId = _taskId;
        
        var settings = _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            _securityKey,
            _teamId,
            true,
            true
        ).Result;
        settings.IsActive = true;

        CommitDbChanges().Wait();
        _queueDao.CompleteAllPending().Wait();
        _clickUpClient.Reset();
    }

    [Fact]
    public async Task ShouldProcessClickUpTimeEntryRequest()
    {
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.ClickUpId);

        await _queueService.PushExternalClientAsync(testContext);
        CommitDbChanges().Wait();
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _clickUpClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.ClickUpId);
    }
    
    [Fact]
    public async Task ShouldDoNothingIfUserDoesNotHaveClickUpConfiguration()
    {
        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var settings = _workspace.GetClickUpSettings(_user.Id);
        settings.IsActive = false;
        
        var timeEntryWithAnotherUser = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user)).First();
        timeEntryWithAnotherUser.TaskId = _taskId;
        await CommitDbChanges();
        
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = timeEntryWithAnotherUser.Id
        };
        Assert.Null(timeEntryWithAnotherUser.ClickUpId);

        await _queueService.PushExternalClientAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(0, _clickUpClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(timeEntryWithAnotherUser);
        Assert.Null(timeEntryWithAnotherUser.ClickUpId);
    }
    
    [Fact]
    public async Task ShouldFillTimeEntryFromTaskDetails()
    {
        _timeEntry.Description = "";
        await CommitDbChanges();
        
        var testContext = new SendSetTimeEntryIntegrationRequestContext()
        {
            TimeEntryId = _timeEntry.Id
        };
        Assert.Null(_timeEntry.ClickUpId);

        await _queueService.PushExternalClientAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(actualProcessedCounter == 1);
        Assert.Equal(1, _clickUpClient.SentTimeEntries.Count);

        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.NotNull(_timeEntry.ClickUpId);
        Assert.NotEmpty(_timeEntry.Description);
    }
}
