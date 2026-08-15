using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToClientTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ISecurityManager _securityManager;
    private readonly IClientSeeder _clientSeeder;
    private readonly IUserDao _userDao;
    private readonly IProjectSeeder _projectSeeder;

    public HasAccessToClientTest(): base()
    {
        _clientSeeder = Scope.Resolve<IClientSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();
        _userDao = Scope.Resolve<IUserDao>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _ownWorkspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfWorkspaceOwner(AccessLevel accessLevel)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(_owner, _ownWorkspace);
        Assert.Equal(MembershipAccessType.Owner, accessType);
        var client = await _clientSeeder.Create(_ownWorkspace);
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, client);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfWorkspaceIfNotMember(AccessLevel accessLevel)
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var client = await _clientSeeder.Create(_ownWorkspace);
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, client);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfClientWasNotSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var client = await _clientSeeder.Create(_ownWorkspace);
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, client);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, client);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHaveNoAccessIfUserHasNoProjectsSharedForClient()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var client = await _clientSeeder.Create(_ownWorkspace);
        await FlushDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, client);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, client);
        Assert.False(hasAccess);
    }

    [Fact]
    public async Task ShouldHaveOnlyReadAccessIfClientProjectWasSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var project = await _projectSeeder.CreateAsync(_ownWorkspace);
        await FlushDbChanges();

        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = project }
            }
        );

        var hasReadAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, project.Client);
        var hasWriteAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, project.Client);

        Assert.True(hasReadAccess);
        Assert.False(hasWriteAccess);
    }
    
    [Fact]
    public async Task ShouldHasReadAndWriteAccessIfUsersRoleIsManager()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var client = await _clientSeeder.Create(_ownWorkspace);
        await FlushDbChanges();

        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.Manager
        );
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, client);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, client);
        Assert.True(hasAccess);
    }
}
