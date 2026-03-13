using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Notification;

public partial class NotificationBgJobService: ITaskNotificationService
{   
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly ITaskDao _taskDao;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IQueueService _queueService;

    public NotificationBgJobService(
        ITaskHistoryItemDao taskHistoryItemDao,
        ITaskDao taskDao,
        INotificationCenterService notificationCenterService,
        IDbSessionProvider sessionProvider,
        IQueueService queueService
    )
    {
        _taskHistoryItemDao = taskHistoryItemDao;
        _taskDao = taskDao;
        _notificationCenterService = notificationCenterService;
        _sessionProvider = sessionProvider;
        _queueService = queueService;
    }
    
    public async Task<bool> SendReminderNotification()
    {
        var tasks = await _taskDao.GetTasksToRemind();
        if (!tasks.Any())
        {
            return false;
        }
        foreach (var task in tasks)
        {
            await _notificationCenterService.Push(
                NotificationActionType.Reminder,
                task.User,
                task
            );
        }
        return true;
    }
}
