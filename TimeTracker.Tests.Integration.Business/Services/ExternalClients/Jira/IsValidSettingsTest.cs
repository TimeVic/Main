using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Jira;

public class IsValidSettingsTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IJiraClient _client;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;

    private readonly string _apiToken;
    private readonly string _userName;
    private readonly string? _url;

    public IsValidSettingsTest() : base(false)
    {
        _client = Scope.Resolve<IJiraClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _apiToken = configuration.GetValue<string>("Integration:Jira:ApiToken");
        _userName = configuration.GetValue<string>("Integration:Jira:UserName");
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

    [Fact]
    public async Task ShouldValidateSettings()
    {
        var isValid = await _client.IsValidClientSettings(_workspace, _user);
        Assert.True(isValid);
    }
    
    [Fact]
    public async Task ShouldNotValidateSettingsIfIncorrectApiKey()
    {
        _workspaceSettingsDao.SetJiraAsync(
            _user,
            _workspace,
            _url,
            "fakeKey",
            _userName,
            true
        ).Wait();
        
        var isValid = await _client.IsValidClientSettings(_workspace, _user);
        Assert.False(isValid);
    }
}
