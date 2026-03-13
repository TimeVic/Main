using Autofac;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Notification.Push.GcmNotificationService;

public class SendTaskReminderNotificationTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;

    public SendTaskReminderNotificationTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _gcmNotificationService = Scope.Resolve<IGcmNotificationService>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task ShouldSendReminder()
    {
        var task2 = await _taskSeeder.CreateAsync(user: _user);
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        await _gcmNotificationService.SendTaskReminderNotification(actualTask);
        
        Assert.Single(FirebaseClientService.SentMessages);
    }
    
    [Fact]
    public async Task ShouldNotSendIfIncorrectToken()
    {
        await _userNotificationTokenDao.Set(_user, "bad token");
        
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(_task.Id);
        await _gcmNotificationService.SendTaskReminderNotification(actualTask);
        await FlushDbChanges();
        
        Assert.False(FirebaseClientService.SentMessages.Any());
        var actualUser = await DbSessionProvider.CurrentSession.GetAsync<UserEntity>(_user.Id);
        Assert.Empty(actualUser.NotificationTokens);
    }
}
