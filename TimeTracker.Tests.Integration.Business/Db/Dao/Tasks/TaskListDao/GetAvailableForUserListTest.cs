using Autofac;
using NHibernate;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tasks.TaskListDao;

public class GetAvailableForUserListTest: BaseTest
{
    private readonly ITaskListDao _taskListDao;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _workspace;

    public GetAvailableForUserListTest(): base()
    {
        _taskListDao = Scope.Resolve<ITaskListDao>();
        _taskListSeeder = Scope.Resolve<ITaskListSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userDao = Scope.Resolve<IUserDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveAllTaskListsForWorkspaceOwner()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _taskListSeeder.CreateSeveralAsync(project, 4);

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            _owner,
            MembershipAccessType.Owner
        );

        Assert.Equal(4, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.Equal(project.Id, item.Project.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveAllTaskListsForWorkspaceManager()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _taskListSeeder.CreateSeveralAsync(project, 3);
        var manager = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, manager, MembershipAccessType.Manager);

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            manager,
            MembershipAccessType.Manager
        );

        Assert.Equal(3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.Equal(project.Id, item.Project.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveOnlyTaskListsFromSharedProjectsForWorkspaceUser()
    {
        var projects = (await _projectSeeder.CreateSeveralAsync(_workspace, 3)).ToList();
        var sharedProject1 = projects.First();
        var sharedProject2 = projects.Last();
        var unavailableProject = projects.Skip(1).First();

        var expectedTaskLists = new List<TaskListEntity>();
        expectedTaskLists.AddRange(await _taskListSeeder.CreateSeveralAsync(sharedProject1, 2));
        expectedTaskLists.AddRange(await _taskListSeeder.CreateSeveralAsync(sharedProject2, 3));
        await _taskListSeeder.CreateSeveralAsync(unavailableProject, 4);

        var user = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new () { Project = sharedProject1 },
                new () { Project = sharedProject2 }
            }
        );

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            user,
            MembershipAccessType.User
        );

        Assert.Equal(expectedTaskLists.Count, actualList.TotalCount);
        Assert.Equal(
            expectedTaskLists.Select(item => item.Id).OrderBy(item => item),
            actualList.Items.Select(item => item.Id).OrderBy(item => item)
        );
        Assert.All(actualList.Items, item =>
        {
            Assert.NotEqual(unavailableProject.Id, item.Project.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveEmptyListForWorkspaceUserWithoutSharedProjects()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _taskListSeeder.CreateSeveralAsync(project, 3);
        var user = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            user,
            MembershipAccessType.User
        );

        Assert.Empty(actualList.Items);
        Assert.Equal(0, actualList.TotalCount);
    }

    [Fact]
    public async Task ShouldNotReceiveArchivedTaskLists()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskLists = (await _taskListSeeder.CreateSeveralAsync(project, 5)).ToList();
        await _taskListDao.ArchiveTaskListAsync(taskLists.First());
        await _taskListDao.ArchiveTaskListAsync(taskLists.Last());

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            _owner,
            MembershipAccessType.Owner
        );

        Assert.Equal(3, actualList.TotalCount);
        Assert.DoesNotContain(actualList.Items, item => item.Id == taskLists.First().Id);
        Assert.DoesNotContain(actualList.Items, item => item.Id == taskLists.Last().Id);
    }

    [Fact]
    public async Task ShouldNotReceiveTaskListsFromArchivedProjects()
    {
        var activeProject = await _projectSeeder.CreateAsync(_workspace);
        var archivedProject = await _projectSeeder.CreateAsync(_workspace);
        var expectedTaskLists = await _taskListSeeder.CreateSeveralAsync(activeProject, 2);
        await _taskListSeeder.CreateSeveralAsync(archivedProject, 3);
        archivedProject.IsArchived = true;

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            _owner,
            MembershipAccessType.Owner
        );

        Assert.Equal(expectedTaskLists.Count, actualList.TotalCount);
        Assert.Equal(
            expectedTaskLists.Select(item => item.Id).OrderBy(item => item),
            actualList.Items.Select(item => item.Id).OrderBy(item => item)
        );
        Assert.DoesNotContain(actualList.Items, item => item.Project.Id == archivedProject.Id);
    }

    [Fact]
    public async Task ShouldLoadProjectAndClientWithoutLazySelects()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        await _taskListSeeder.CreateSeveralAsync(project, 3);

        await FlushDbChanges(isClearSession: true);
        var actualList = await _taskListDao.GetAvailableForUserListAsync(
            _workspace,
            _owner,
            MembershipAccessType.Owner
        );

        Assert.Equal(3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(NHibernateUtil.IsInitialized(item.Project));
            Assert.True(NHibernateUtil.IsInitialized(item.Project.Client));
            Assert.NotNull(item.Project.Client);
        });
    }
}
