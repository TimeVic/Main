using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Notification;

public partial class TaskNotificationService: ITaskNotificationService
{   
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly ITaskDao _taskDao;
    private readonly IGcmNotificationService _notificationService;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IQueueService _queueService;

    public TaskNotificationService(
        ITaskHistoryItemDao taskHistoryItemDao,
        ITaskDao taskDao,
        IGcmNotificationService notificationService,
        IDbSessionProvider sessionProvider,
        IQueueService queueService
    )
    {
        _taskHistoryItemDao = taskHistoryItemDao;
        _taskDao = taskDao;
        _notificationService = notificationService;
        _sessionProvider = sessionProvider;
        _queueService = queueService;
    }
}
