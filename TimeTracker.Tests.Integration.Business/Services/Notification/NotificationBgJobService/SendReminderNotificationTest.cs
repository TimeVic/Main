using Autofac;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Notification;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Notification.NotificationBgJobService;

public class SendReminderNotificationTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly ITaskNotificationService _notificationBgJobService;
    private readonly INotificationCenterService _notificationCenterService;

    public SendReminderNotificationTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _notificationBgJobService = Scope.Resolve<ITaskNotificationService>();
        _gcmNotificationService = Scope.Resolve<IGcmNotificationService>();
        _notificationCenterService = Scope.Resolve<INotificationCenterService>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task ShouldSendReminder()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await CommitDbChanges();

        // Act
        await _notificationBgJobService.SendReminderNotification();
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
    
    [Fact]
    public async Task ShouldNotSendIfTimeHasNotCome()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(+1);
        await CommitDbChanges();

        // Act
        await _notificationBgJobService.SendReminderNotification();
        
        // Assert
        Assert.Empty(FirebaseClientService.SentMessages);
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
    
    [Fact]
    public async Task ShouldNotSendIfRemindTimeWasNotProvided()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        _task.ReminderTime = null;
        await CommitDbChanges();

        // Act
        await _notificationBgJobService.SendReminderNotification();
        
        // Assert
        Assert.Empty(FirebaseClientService.SentMessages);
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
    
    [Fact]
    public async Task ShouldNotSendNotificationTwice()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await CommitDbChanges();

        // Act
        await _notificationBgJobService.SendReminderNotification();
        await _notificationCenterService.MarkAllAsRead(_user, _task.Workspace);
        
        await _notificationBgJobService.SendReminderNotification();
        
        // Assert
        Assert.Single(FirebaseClientService.SentMessages);
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
}
