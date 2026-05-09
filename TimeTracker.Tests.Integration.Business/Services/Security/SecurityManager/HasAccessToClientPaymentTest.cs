using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToClientPaymentTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ISecurityManager _securityManager;
    private readonly IClientPaymentSeeder _paymentSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserDao _userDao;

    public HasAccessToClientPaymentTest(): base()
    {
        _paymentSeeder = Scope.Resolve<IClientPaymentSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();
        _userDao = Scope.Resolve<IUserDao>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _ownWorkspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
        _queueDao.CompleteAllPending().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfWorkspaceOwner(AccessLevel accessLevel)
    {
        var payment = await CreatePaymentAsync();

        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, payment);

        Assert.True(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfWorkspaceManager(AccessLevel accessLevel)
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, otherUser, MembershipAccessType.Manager);
        await FlushDbChanges();
        var payment = await CreatePaymentAsync();

        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, payment);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task ShouldHasOnlyReadAccessIfWorkspaceUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_ownWorkspace, otherUser, MembershipAccessType.User);
        await FlushDbChanges();
        var payment = await CreatePaymentAsync();

        var hasReadAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, payment);
        var hasWriteAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, payment);

        Assert.True(hasReadAccess);
        Assert.False(hasWriteAccess);
    }

    [Fact]
    public async Task ShouldHasNoAccessIfWorkspaceWasNotSharedForUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var payment = await CreatePaymentAsync();

        var hasReadAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, payment);
        var hasWriteAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, payment);

        Assert.False(hasReadAccess);
        Assert.False(hasWriteAccess);
    }

    private async Task<ClientPaymentEntity> CreatePaymentAsync()
    {
        var project = await _projectSeeder.CreateAsync(_ownWorkspace);
        return (await _paymentSeeder.CreateSeveralAsync(project.Client, project)).First();
    }
}
