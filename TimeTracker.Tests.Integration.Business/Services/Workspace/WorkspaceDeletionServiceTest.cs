using Autofac;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Workspace;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Workspace;

public class WorkspaceDeletionServiceTest : BaseTest
{
    private readonly IWorkspaceDeletionService _workspaceDeletionService;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _defaultWorkspace;

    public WorkspaceDeletionServiceTest() : base()
    {
        _workspaceDeletionService = Scope.Resolve<IWorkspaceDeletionService>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _userDao = Scope.Resolve<IUserDao>();
        _workspaceSeeder = Scope.Resolve<IWorkspaceSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _user = Scope.Resolve<IUserSeeder>().CreateActivatedAsync().Result;
        _defaultWorkspace = _userDao.GetDefaultWorkspace(_user).Result;
    }

    [Fact]
    public async Task ShouldSoftDeleteAndHideWorkspace()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        await _workspaceDeletionService.SoftDeleteAsync(workspace);
        await FlushDbChanges(true);

        var deletedWorkspace = await DbSessionProvider.CurrentSession.GetAsync<WorkspaceEntity>(workspace.Id);
        Assert.NotNull(deletedWorkspace);
        Assert.NotNull(deletedWorkspace.DeletedAt);
        Assert.Null(await _workspaceDao.GetById(workspace.Id));
        Assert.DoesNotContain(await _userDao.GetUsersWorkspaces(_user), item => item.Id == workspace.Id);
    }

    [Fact]
    public async Task ShouldHardDeleteSoftDeletedWorkspace()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        await _workspaceDeletionService.SoftDeleteAsync(workspace);
        await _workspaceDeletionService.HardDeleteAsync(workspace);
        await FlushDbChanges(true);

        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<WorkspaceEntity>(workspace.Id));
    }

    [Fact]
    public async Task ShouldHardDeleteWorkspaceRelatedData()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        var project = await _projectSeeder.CreateAsync(workspace);
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(workspace, _user, project: project)).Single();
        await FlushDbChanges(true);

        var workspaceToDelete = await DbSessionProvider.CurrentSession.GetAsync<WorkspaceEntity>(workspace.Id);
        await _workspaceDeletionService.SoftDeleteAsync(workspaceToDelete!);
        await FlushDbChanges(true);

        var deletedWorkspace = await _workspaceDao.GetDeletedByIdAsync(workspace.Id);
        await _workspaceDeletionService.HardDeleteAsync(deletedWorkspace!);
        await FlushDbChanges(true);

        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<WorkspaceEntity>(workspace.Id));
        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<ProjectEntity>(project.Id));
        Assert.Null(await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id));
    }

    [Fact]
    public async Task ShouldNotSoftDeleteDefaultWorkspace()
    {
        await Assert.ThrowsAsync<DataValidationException>(
            () => _workspaceDeletionService.SoftDeleteAsync(_defaultWorkspace)
        );
    }
}
