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

public class ShareAccessTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IProjectDao _projectDao;
    private readonly IUserDao _userDao;

    public ShareAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldProvideNewAccess_User()
    {
        var expectedAccess = MembershipAccessType.User;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess,
            new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject1 }
            });
        await DbSessionProvider.PerformCommitAsync();
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Id, actualMembership.User.Id);
        Assert.Equal(1, actualMembership.ProjectAccesses.Count);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.Project.Id == expectedProject1.Id);
        Assert.DoesNotContain(actualMembership.ProjectAccesses, access => access.Project.Id == project2.Id);
    }
    
    [Fact]
    public async Task ShouldUpdateAccess_User()
    {
        var expectedHourlyRate1 = 12;
        var expectedHourlyRate2 = 13;
        var expectedAccess = MembershipAccessType.User;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess,
            new List<ProjectAccessModel>()
            {
                new ()
                {
                    Project = project2,
                    HourlyRate = expectedHourlyRate2
                }
            });
        await DbSessionProvider.PerformCommitAsync();
        
        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess,
            new List<ProjectAccessModel>()
            {
                new ()
                {
                    Project = expectedProject1,
                    HourlyRate = expectedHourlyRate1
                }
            });
        await DbSessionProvider.PerformCommitAsync();
        
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Id, actualMembership.User.Id);
        Assert.Equal(1, actualMembership.ProjectAccesses.Count);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.Project.Id == expectedProject1.Id);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.HourlyRate == expectedHourlyRate1);
        Assert.DoesNotContain(actualMembership.ProjectAccesses, access => access.Project.Id == project2.Id);
        Assert.DoesNotContain(actualMembership.ProjectAccesses, access => access.HourlyRate == expectedHourlyRate2);
    }
    
    [Fact]
    public async Task ShouldProvideNewBaseAccess_Manager()
    {
        var expectedHourlyRate = 13;
        var expectedAccess = MembershipAccessType.Manager;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess,
            new List<ProjectAccessModel>()
            {
                new ()
                {
                    Project = expectedProject1,
                    HourlyRate = expectedHourlyRate
                }
            });
        await DbSessionProvider.PerformCommitAsync();
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Id, actualMembership.User.Id);
        Assert.Equal(1, actualMembership.ProjectAccesses.Count);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.HourlyRate == expectedHourlyRate);
    }
    
    [Fact]
    public async Task ShouldProvideNewBaseAccessWithoutProjects_Manager()
    {
        var expectedAccess = MembershipAccessType.Manager;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser, expectedAccess);
        await DbSessionProvider.PerformCommitAsync();
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Id, actualMembership.User.Id);
        Assert.Equal(0, actualMembership.ProjectAccesses.Count);
    }
}
