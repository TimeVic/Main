using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Project;

public class DeleteTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;

    public DeleteTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldSoftDeleteProject()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await FlushDbChanges();

        await _projectDao.SoftDeleteAsync(project);
        await FlushDbChanges();
        
        Assert.NotNull(project.DeletedAt);
    }

    [Fact]
    public async Task ShouldReceiveSoftDeletedProjectById()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        project.DeletedAt = DateTime.UtcNow;
        await FlushDbChanges();

        var actualProject = await _projectDao.GetById(project.Id);

        Assert.NotNull(actualProject);
        Assert.Equal(project.Id, actualProject.Id);
    }
}
