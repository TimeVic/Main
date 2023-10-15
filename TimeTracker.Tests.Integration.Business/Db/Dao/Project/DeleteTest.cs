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
using TimeTracker.Business.Testing.Seeders.Entity.Task;
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
    private readonly ITaskListSeeder _taskListSeeder;

    public DeleteTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _userDao = Scope.Resolve<IUserDao>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldArchiveTasksListWithProject()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _taskListSeeder.CreateSeveralAsync(project, 3);
        await CommitDbChanges();

        await _projectDao.ArchiveProject(project);
        await DbSessionProvider.CurrentSession.RefreshAsync(project);
        
        Assert.True(project.IsArchived);
        
        Assert.All(project.TaskLists, item =>
        {
            Assert.True(item.IsArchived);
        });
    }
}
