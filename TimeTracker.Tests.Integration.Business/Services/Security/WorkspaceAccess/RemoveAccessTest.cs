using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.WorkspaceAccess;

public class RemoveAccessTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IProjectDao _projectDao;

    public RemoveAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldRemoveAccess()
    {
        var expectedAccess = MembershipAccessType.User;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess,
            new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject1 }
            });
        await DbSessionProvider.PerformCommitAsync();
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);

        var isRemoved = await _workspaceAccessService.RemoveAccessAsync(actualMembership.Id);
        Assert.True(isRemoved);
        await DbSessionProvider.PerformCommitAsync();

        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        Assert.DoesNotContain(_workspace.Memberships, item => item.User.Id == expectedUser.Id);
    }
}
