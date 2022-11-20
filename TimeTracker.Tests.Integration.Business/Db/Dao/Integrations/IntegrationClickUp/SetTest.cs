using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Integrations.IntegrationClickUp;

public class SetTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly IDataFactory<WorkspaceSettingsClickUpEntity> _factory;
    private readonly IUserDao _userDao;

    public SetTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _factory = Scope.Resolve<IDataFactory<WorkspaceSettingsClickUpEntity>>();
        _workspaceSettingsDao = Scope.Resolve<IWorkspaceSettingsDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public void WorkspaceShouldNotHaveActiveIntegration()
    {
        Assert.False(_workspace.IsIntegrationClickUpActive(_user.Id));
    }
    
    [Fact]
    public async Task ShouldCreate()
    {
        var expectIntegration = _factory.Generate(); 
        
        var actualIntegration = await _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            expectIntegration.SecurityKey,
            expectIntegration.TeamId,
            expectIntegration.IsCustomTaskIds
        );

        await DbSessionProvider.CurrentSession.RefreshAsync(actualIntegration);
        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);

        Assert.True(actualIntegration.Id > 0);
        Assert.Equal(_workspace.Id, actualIntegration.Workspace.Id);
        Assert.Equal(expectIntegration.SecurityKey, actualIntegration.SecurityKey);
        Assert.Equal(expectIntegration.TeamId, actualIntegration.TeamId);
        Assert.Equal(expectIntegration.IsCustomTaskIds, actualIntegration.IsCustomTaskIds);
        
        Assert.NotNull(actualIntegration.Workspace);
        Assert.NotNull(_workspace.GetClickUpSettings(_user.Id));
        Assert.True(_workspace.IsIntegrationClickUpActive(_user.Id));
    }
    
    [Fact]
    public async Task ShouldUpdateExists()
    {
        var fakeIntegration = _factory.Generate();
        
        var expectIntegration = await _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            fakeIntegration.SecurityKey,
            fakeIntegration.TeamId,
            fakeIntegration.IsCustomTaskIds
        );
        
        var expectIntegrationTemplate = _factory.Generate();
        var actualIntegration = await _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            expectIntegrationTemplate.SecurityKey,
            expectIntegrationTemplate.TeamId,
            expectIntegrationTemplate.IsCustomTaskIds
        );
        await CommitDbChanges();
        
        Assert.Equal(expectIntegration.Id, actualIntegration.Id);
        Assert.Equal(_workspace.Id, actualIntegration.Workspace.Id);
        Assert.Equal(expectIntegration.SecurityKey, actualIntegration.SecurityKey);
        Assert.Equal(expectIntegration.TeamId, actualIntegration.TeamId);
        Assert.Equal(expectIntegration.IsCustomTaskIds, actualIntegration.IsCustomTaskIds);
    }
}
