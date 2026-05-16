using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Notification;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Notification.NotificationCenterService.Task;

public partial class TaskNotificationHandlerTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly ITaskNotificationService _notificationBgJobService;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IQueueService _queueService;

    public TaskNotificationHandlerTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _notificationBgJobService = Scope.Resolve<ITaskNotificationService>();
        _gcmNotificationService = Scope.Resolve<IGcmNotificationService>();
        _notificationCenterService = Scope.Resolve<INotificationCenterService>();
        _queueService = Scope.Resolve<IQueueService>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }
}
