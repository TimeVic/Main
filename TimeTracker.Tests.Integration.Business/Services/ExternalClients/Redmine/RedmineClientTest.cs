using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Redmine;

public class RedmineClientTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly IRedmineClient _redmineClient;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;
    
    private readonly string _apiKey;
    private readonly long _userId;
    private readonly string _taskId;
    private readonly string? _redmineUrl;
    private readonly long _activityId;

    public RedmineClientTest() : base(false)
    {
        _redmineClient = Scope.Resolve<IRedmineClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _apiKey = configuration.GetValue<string>("Integration:Redmine:ApiKey");
        _userId = configuration.GetValue<long>("Integration:Redmine:UserId");
        _taskId = configuration.GetValue<string>("Integration:Redmine:TaskId");
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
    }

    [Fact]
    public async Task ShouldSendNewTimeEntry()
    {
        var expectedDescription = "Test description";
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date,
            TimeSpan.FromMinutes(1),
            true,
            description: expectedDescription
        );
        activeEntry.TaskId = _taskId.ToString();
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotEmpty(actualResponse.Id);
        Assert.Equal(expectedDescription, actualResponse.Comment);

        activeEntry.RedmineId = actualResponse.Id;
        var isDeleted = await _redmineClient.DeleteTimeEntryAsync(activeEntry);
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
    
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
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
            TimeSpan.FromMinutes(1),
            true
        );
        activeEntry.TaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var creatingResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.False(creatingResponse.IsError);
        activeEntry.RedmineId = creatingResponse.Id;
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
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.False(actualResponse.IsError);
        
        var isDeleted = await _redmineClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }
}
