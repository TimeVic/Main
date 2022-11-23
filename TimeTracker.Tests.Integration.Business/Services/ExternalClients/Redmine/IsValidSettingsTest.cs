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

public class IsValidSettingsTest : BaseTest
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

    public IsValidSettingsTest() : base(false)
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
    public async Task ShouldValidateSettings()
    {
        var isValid = await _redmineClient.IsValidClientSettings(_workspace, _user);
        Assert.True(isValid);
    }
    
    [Fact]
    public async Task ShouldNotValidateSettingsIfIncorrectApiKey()
    {
        _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _workspace,
            _redmineUrl,
            "fakeUrl",
            _userId,
            _activityId
        ).Wait();
        
        var isValid = await _redmineClient.IsValidClientSettings(_workspace, _user);
        Assert.False(isValid);
    }
    
    [Fact]
    public async Task ShouldNotValidateSettingsIfIncorrectUrl()
    {
        _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _workspace,
            "http://bla.cla.com",
            _apiKey,
            _userId,
            _activityId
        ).Wait();
        
        var isValid = await _redmineClient.IsValidClientSettings(_workspace, _user);
        Assert.False(isValid);
    }
}
