using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToProjectTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ISecurityManager _securityManager;
    private readonly IProjectDao _projectDao;

    public HasAccessToProjectTest(): base()
    {
        _projectDao = Scope.Resolve<IProjectDao>();
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
        var project = await _projectDao.CreateAsync(_ownWorkspace, "Test 1");
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, project);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfWorkspaceIfNotMember(AccessLevel accessLevel)
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var project = await _projectDao.CreateAsync(_ownWorkspace, "Test 1");
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, project);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfProjectWasNotSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var project = await _projectDao.CreateAsync(_ownWorkspace, "Test 1");

        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, project);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, project);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasOnlyReadAccessIfProjectWasSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var project = await _projectDao.CreateAsync(_ownWorkspace, "Test 1");
        await CommitDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectEntity> { project }
        );
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, project);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, project);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasReadAndWriteAccessIfUsersRoleIsManager()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var project = await _projectDao.CreateAsync(_ownWorkspace, "Test 1");
        await CommitDbChanges();

        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.Manager
        );
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, project);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, project);
        Assert.True(hasAccess);
    }
}
