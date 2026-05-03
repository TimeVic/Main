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

public class HasAccessToMemberPaymentTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ISecurityManager _securityManager;
    private readonly IProjectDao _projectDao;
    private readonly IClientDao _clientDao;
    private readonly IMemberPaymentSeeder _paymentSeeder;
    private readonly IUserDao _userDao;

    public HasAccessToMemberPaymentTest(): base()
    {
        _paymentSeeder = Scope.Resolve<IMemberPaymentSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _clientDao = Scope.Resolve<IClientDao>();
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
    public async Task PaymentOwnerHasAccess(AccessLevel accessLevel)
    {
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(_owner, _ownWorkspace);
        Assert.Equal(MembershipAccessType.Owner, accessType);
        var payment = (await _paymentSeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, payment);
        Assert.True(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ManagerHasAccessToWorkspaceMemberPayment(AccessLevel accessLevel)
    {
        var manager = await _userSeeder.CreateActivatedAndShareAsync(
            _ownWorkspace,
            MembershipAccessType.Manager
        );
        var payment = (await _paymentSeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();

        var hasAccess = await _securityManager.HasAccess(accessLevel, manager, payment);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task ShouldHasNoAccessIfWorkspaceWasNotSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var payment = (await _paymentSeeder.CreateSeveralAsync(_ownWorkspace, _owner)).First();

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, payment);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, payment);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasAccessIfWorkspaceWasSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();

        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );
        await FlushDbChanges();

        var payment = (await _paymentSeeder.CreateSeveralAsync(_ownWorkspace, otherUser)).First();
        
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, payment);
        Assert.True(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, payment);
        Assert.True(hasAccess);
    }
}
