using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToNotificationTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _owner;
    private readonly WorkspaceEntity _ownWorkspace;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ISecurityManager _securityManager;
    private IUserDao _userDao;
    private readonly IGoalsTrackerSeeder _goalsTrackerSeeder;
    private readonly ITaskSeeder _taskSeeder;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly TaskEntity _task;

    public HasAccessToNotificationTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _securityManager = Scope.Resolve<ISecurityManager>();
        _goalsTrackerSeeder = Scope.Resolve<IGoalsTrackerSeeder>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userDao = Scope.Resolve<IUserDao>();
        _notificationCenterService = Scope.Resolve<INotificationCenterService>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _ownWorkspace = _userDao.GetUsersWorkspaces(_owner, MembershipAccessType.Owner).Result.First();
        _task = _taskSeeder.CreateAsync(user: _owner).Result;
        _task.ReminderTime = DateTime.UtcNow;
        FlushDbChanges().Wait();
    }

    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldHasFullAccessIfOwner(AccessLevel accessLevel)
    {
        await _notificationCenterService.Push(NotificationActionType.Reminder, _owner, _task);
        await FlushDbChanges();
        var notifications = await _notificationCenterService.GetList(_owner, _task.Workspace);
        var notification = notifications.Items.First();
        
        var accessType = await _workspaceAccessService.GetAccessTypeAsync(_owner, _ownWorkspace);
        Assert.Equal(MembershipAccessType.Owner, accessType);
        var hasAccess = await _securityManager.HasAccess(accessLevel, _owner, notification);
        Assert.True(hasAccess);
    }
    
    [Theory]
    [InlineData(AccessLevel.Read)]
    [InlineData(AccessLevel.Write)]
    public async Task ShouldNoAccessIfUserIsNotMemberOfWorkspace(AccessLevel accessLevel)
    {
        await _notificationCenterService.Push(NotificationActionType.Reminder, _owner, _task);
        await FlushDbChanges();
        var notifications = await _notificationCenterService.GetList(_owner, _task.Workspace);
        var notification = notifications.Items.First();
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _securityManager.HasAccess(accessLevel, otherUser, notification);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfUserIsMemberWithManagerRole()
    {
        await _notificationCenterService.Push(NotificationActionType.Reminder, _owner, _task);
        await FlushDbChanges();
        var notifications = await _notificationCenterService.GetList(_owner, _task.Workspace);
        var notification = notifications.Items.First();
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await FlushDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.Manager
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, notification);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, notification);
        Assert.False(hasAccess);
    }
    
    [Fact]
    public async Task ShouldHasNoAccessIfUserIsMemberWithManagerUser()
    {
        await _notificationCenterService.Push(NotificationActionType.Reminder, _owner, _task);
        await FlushDbChanges();
        var notifications = await _notificationCenterService.GetList(_owner, _task.Workspace);
        var notification = notifications.Items.First();
        
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await FlushDbChanges();
       
        await _workspaceAccessService.ShareAccessAsync(
            _ownWorkspace,
            otherUser,
            MembershipAccessType.User
        );

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, notification);
        Assert.False(hasAccess);
        
        hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, notification);
        Assert.False(hasAccess);
    }
}
