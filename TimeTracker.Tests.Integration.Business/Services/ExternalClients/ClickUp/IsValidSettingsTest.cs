using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public class IsValidSettingsTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IClickUpClient _сlickUpClient;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly string _securityKey;

    private readonly string _teamId;
    private readonly string _taskId;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;

    public IsValidSettingsTest() : base(false)
    {
        _сlickUpClient = Scope.Resolve<IClickUpClient>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();

        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userDao = Scope.Resolve<IUserDao>();

        var configuration = Scope.Resolve<IConfiguration>();
        _securityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _teamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _taskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");

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

    [Fact]
    public async Task ShouldValidateSettings()
    {
        var isValid = await _сlickUpClient.IsValidClientSettings(_workspace, _user);
        Assert.True(isValid);
    }
    
    [Fact]
    public async Task ShouldNotValidateSettingsIfIncorrectApiKey()
    {
        _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            "fakeKey",
            _teamId,
            true,
            true
        ).Wait();
        
        var isValid = await _сlickUpClient.IsValidClientSettings(_workspace, _user);
        Assert.False(isValid);
    }
}
