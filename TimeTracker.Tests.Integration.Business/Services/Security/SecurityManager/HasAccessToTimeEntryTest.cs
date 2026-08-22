using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToTimeEntryTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ISecurityManager _securityManager;
    private IUserDao _userDao;

    public HasAccessToTimeEntryTest(): base()
    {
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();
        _userDao = Scope.Resolve<IUserDao>();

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
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, timeEntry);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfWorkspaceIfNotMember(AccessLevel accessLevel)
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, timeEntry);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasOnlyReadAccessIfUsersRoleIsUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();

        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, otherUser, MembershipAccessType.User);
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, timeEntry);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, timeEntry);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasReadAndWriteAccessIfUsersRoleIsManager()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();

        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, otherUser, MembershipAccessType.Manager);
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, timeEntry);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, timeEntry);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasReadAndWriteAccessIfUsersIsTimeEntryOwner()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, otherUser)).First();

        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, otherUser, MembershipAccessType.User);
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, timeEntry);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, timeEntry);
        Assert.True(hasAccess);
    }

    [Fact]
    public async Task ShouldLockPendingAndApprovedEntriesWhenApprovalsEnabledInTeamMode()
    {
        var developer = await _userSeeder.CreateActivatedAsync();
        _ownWorkspace.Mode = WorkspaceMode.Team;
        _ownWorkspace.IsApprovalsEnabled = true;

        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, developer, MembershipAccessType.User);

        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, developer)).First();
        timeEntry.Status = TimeEntryStatus.Pending;

        // Neither owner nor developer can write to Pending
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, _owner, timeEntry));
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, developer, timeEntry));

        // Neither owner nor developer can write to Approved
        timeEntry.Status = TimeEntryStatus.Approved;
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, _owner, timeEntry));
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, developer, timeEntry));

        // Draft and Rejected can be modified only by author
        timeEntry.Status = TimeEntryStatus.Draft;
        Assert.True(await _securityManager.HasAccess(AccessLevel.Write, developer, timeEntry));
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, _owner, timeEntry));

        timeEntry.Status = TimeEntryStatus.Rejected;
        Assert.True(await _securityManager.HasAccess(AccessLevel.Write, developer, timeEntry));
        Assert.False(await _securityManager.HasAccess(AccessLevel.Write, _owner, timeEntry));
    }

    [Fact]
    public async Task ShouldAllowOwnerToWriteApprovedInSoloMode()
    {
        _ownWorkspace.Mode = WorkspaceMode.Solo;
        _ownWorkspace.IsApprovalsEnabled = false;

        var timeEntry = (await _timeEntrySeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();
        timeEntry.Status = TimeEntryStatus.Approved;

        Assert.True(await _securityManager.HasAccess(AccessLevel.Write, _owner, timeEntry));
    }
}
