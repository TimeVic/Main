using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToGoalsTrackerTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ISecurityManager _securityManager;
    private readonly IProjectDao _projectDao;
    private readonly IClientDao _clientDao;
    private readonly IWorkspaceDao _workspaceDao;
    private IUserDao _userDao;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;

    public HasAccessToGoalsTrackerTest(): base()
    {
        _projectDao = Scope.Resolve<IProjectDao>();
        _clientDao = Scope.Resolve<IClientDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();
        _goalsTrackerSeeder = Scope.Resolve<IGoalsTrackerSeeder>();
        _userDao = Scope.Resolve<IUserDao>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _ownWorkspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasFullAccessIfOwner(AccessLevel accessLevel)
    {
        var goalsTracker = await _goalsTrackerSeeder.CreateAsync(_owner, _ownWorkspace);
        
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(_owner, _ownWorkspace);
        Assert.Equal(MembershipAccessType.Owner, accessType);
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, goalsTracker);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfUserIsNotMemberOfWorkspace(AccessLevel accessLevel)
    {
        var goalsTracker = await _goalsTrackerSeeder.CreateAsync(_owner, _ownWorkspace);
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, goalsTracker);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfUserIsMemberWithManagerRole()
    {
        var goalsTracker = await _goalsTrackerSeeder.CreateAsync(_owner, _ownWorkspace);
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await CommitDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.Manager
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, goalsTracker);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, goalsTracker);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfUserIsMemberWithManagerUser()
    {
        var goalsTracker = await _goalsTrackerSeeder.CreateAsync(_owner, _ownWorkspace);
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await CommitDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, goalsTracker);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, goalsTracker);
        Assert.False(hasAccess);
    }
}
