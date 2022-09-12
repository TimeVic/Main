using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Client;

public class HasAccessTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<ClientEntity> _clientFactory;
    private readonly IClientDao _clientDao;

    public HasAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _clientDao = Scope.Resolve<IClientDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _clientFactory = Scope.Resolve<IDataFactory<ClientEntity>>();
    }

    [Fact]
    public async Task ShouldReturnTrueIfHasAccess()
    {
        var fakeProject = _clientFactory.Generate();
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = user.Workspaces.First();
        var expectProject = await _clientDao.CreateAsync(expectWorkspace, fakeProject.Name);
        await DbSessionProvider.PerformCommitAsync();
        
        var hasAccess = await _clientDao.HasAccessAsync(user, expectProject);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnTrueIfItFromAnotherWorkspace()
    {
        var fakeProject = _clientFactory.Generate();

        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(user, "New");
        
        var expectedProject = await _clientDao.CreateAsync(expectWorkspace, fakeProject.Name);
        
        var hasAccess = await _clientDao.HasAccessAsync(user, expectedProject);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnFalseIfNotOwner()
    {
        var fakeProject = _clientFactory.Generate();
        
        var ownedUser = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(ownedUser, "New");
        
        var expectedProject = await _clientDao.CreateAsync(expectWorkspace, fakeProject.Name);
        
        var anotherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _clientDao.HasAccessAsync(anotherUser, expectedProject);
        Assert.False(hasAccess);
    }
}
