using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Project;

public class HasAccessTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly IProjectDao _projectDao;

    public HasAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _projectFactory = Scope.Resolve<IDataFactory<ProjectEntity>>();
    }

    [Fact]
    public async Task ShouldReturnTrueIfHasAccess()
    {
        var fakeProject = _projectFactory.Generate();
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = user.Workspaces.First();
        var expectProject = await _projectDao.CreateAsync(expectWorkspace, fakeProject.Name);
        await DbSessionProvider.PerformCommitAsync();
        
        var hasAccess = await _projectDao.HasAccessAsync(user, expectProject);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnTrueIfItFromAnotherWorkspace()
    {
        var fakeProject = _projectFactory.Generate();

        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(user, "New");
        
        var expectedProject = await _projectDao.CreateAsync(expectWorkspace, fakeProject.Name);
        
        var hasAccess = await _projectDao.HasAccessAsync(user, expectedProject);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnFalseIfNotOwner()
    {
        var fakeProject = _projectFactory.Generate();
        
        var ownedUser = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(ownedUser, "New");
        
        var expectedProject = await _projectDao.CreateAsync(expectWorkspace, fakeProject.Name);
        
        var anotherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _projectDao.HasAccessAsync(anotherUser, expectedProject);
        Assert.False(hasAccess);
    }
}
