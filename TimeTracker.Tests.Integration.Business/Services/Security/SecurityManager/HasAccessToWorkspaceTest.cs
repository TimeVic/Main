using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToWorkspaceTest: BaseTest
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

    public HasAccessToWorkspaceTest(): base()
    {
        _projectDao = Scope.Resolve<IProjectDao>();
        _clientDao = Scope.Resolve<IClientDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _ownWorkspace = _owner.Workspaces.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfWorkspaceOwner(AccessLevel accessLevel)
    {
        Assert.True(_ownWorkspace.IsOwner(_owner));
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, _ownWorkspace);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfUserIsNotMemberOfWorkspace(AccessLevel accessLevel)
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, _ownWorkspace);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasAccessIfUserIsMemberWithManagerRole()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await CommitDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.Manager
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, _ownWorkspace);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, _ownWorkspace);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasAccessIfUserIsMemberWithManagerUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await CommitDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, _ownWorkspace);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, _ownWorkspace);
        Assert.False(hasAccess);
    }
}
