using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Notification;

public partial class TaskNotificationService: ITaskNotificationService
{   
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly ITaskDao _taskDao;
    private readonly IQueueService _queueService;

    public TaskNotificationService(
        ITaskHistoryItemDao taskHistoryItemDao,
        ITaskDao taskDao,
        IQueueService queueService
    )
    {
        _taskHistoryItemDao = taskHistoryItemDao;
        _taskDao = taskDao;
        _queueService = queueService;
    }
}
