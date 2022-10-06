using Autofac;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.TimeEntry;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.WorkspaceAccess;

public class ShareAccessByEmailTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IProjectDao _projectDao;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;

    public ShareAccessByEmailTest(): base()
    {
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _queueService = Scope.Resolve<IQueueService>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldProvideAccessAndCreateNewUser()
    {
        var expectedAccess = MembershipAccessType.User;
        var expectedUser = _userFactory.Generate();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser.Email, expectedAccess,
            new List<ProjectEntity>()
            {
                expectedProject1
            });
        await DbSessionProvider.PerformCommitAsync();
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Email.ToLower(), actualMembership.User.Email);
        Assert.Equal(1, actualMembership.ProjectAccesses.Count);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.Project.Id == expectedProject1.Id);
        Assert.DoesNotContain(actualMembership.ProjectAccesses, access => access.Project.Id == project2.Id);
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(expectedUser.Email.ToLower(), actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldProvideAccessForActiveUserByEmail()
    {
        var expectedAccess = MembershipAccessType.User;
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var expectedProject1 = await _projectDao.CreateAsync(_workspace, "Test 1");
        var project2 = await _projectDao.CreateAsync(_workspace, "Test 2");
        await DbSessionProvider.PerformCommitAsync();

        var actualMembership = await _workspaceAccessService.ShareAccessAsync(_workspace, expectedUser.Email, expectedAccess,
            new List<ProjectEntity>()
            {
                expectedProject1
            });
        await DbSessionProvider.PerformCommitAsync();
        
        Assert.Equal(expectedAccess, actualMembership.Access);
        Assert.Equal(_workspace.Id, actualMembership.Workspace.Id);
        Assert.Equal(expectedUser.Email.ToLower(), actualMembership.User.Email);
        Assert.Equal(1, actualMembership.ProjectAccesses.Count);
        Assert.Contains(actualMembership.ProjectAccesses, access => access.Project.Id == expectedProject1.Id);
        Assert.DoesNotContain(actualMembership.ProjectAccesses, access => access.Project.Id == project2.Id);
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(expectedUser.Email.ToLower(), actualEmail.To);
    }
}
