using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Business.Orm.Dao.Notes;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Notes;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToNoteTest : BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IUserDao _userDao;
    private readonly INoteDao _noteDao;
    private readonly ISecurityManager _securityManager;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _workspace;

    public HasAccessToNoteTest() : base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userDao = Scope.Resolve<IUserDao>();
        _noteDao = Scope.Resolve<INoteDao>();
        _securityManager = Scope.Resolve<ISecurityManager>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
        _queueDao.CompleteAllPending().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfOwnerCreatedPrivateNote(AccessLevel accessLevel)
    {
        var note = await CreateNoteAsync(NoteVisibility.Private, _owner);

        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, note);

        Assert.True(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasAccessIfManagerReadsWorkspaceNote(AccessLevel accessLevel)
    {
        var manager = await _userSeeder.CreateActivatedAndShareAsync(_workspace, MembershipAccessType.Manager);
        var note = await CreateNoteAsync(NoteVisibility.Workspace, _owner);

        var hasAccess = await _securityManager.HasAccess(accessLevel, manager, note);

        Assert.True(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasNoAccessIfManagerReadsPrivateNoteCreatedByOtherUser(AccessLevel accessLevel)
    {
        var manager = await _userSeeder.CreateActivatedAndShareAsync(_workspace, MembershipAccessType.Manager);
        var note = await CreateNoteAsync(NoteVisibility.Private, _owner);

        var hasAccess = await _securityManager.HasAccess(accessLevel, manager, note);

        Assert.False(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasNoAccessIfUserRoleReadsWorkspaceNote(AccessLevel accessLevel)
    {
        var user = await _userSeeder.CreateActivatedAndShareAsync(_workspace, MembershipAccessType.User);
        var note = await CreateNoteAsync(NoteVisibility.Workspace, _owner);

        var hasAccess = await _securityManager.HasAccess(accessLevel, user, note);

        Assert.False(hasAccess);
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasNoAccessIfUserIsNotWorkspaceMember(AccessLevel accessLevel)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var note = await CreateNoteAsync(NoteVisibility.Workspace, _owner);

        var hasAccess = await _securityManager.HasAccess(accessLevel, user, note);

        Assert.False(hasAccess);
    }

    private async Task<NoteNodeEntity> CreateNoteAsync(NoteVisibility visibility, UserEntity createdByUser)
    {
        return await _noteDao.CreateNodeAsync(
            _workspace,
            null,
            createdByUser,
            NoteNodeType.Document,
            "Security note",
            "# Security note",
            visibility,
            1000
        );
    }
}
