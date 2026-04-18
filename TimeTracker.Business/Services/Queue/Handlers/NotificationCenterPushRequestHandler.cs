using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.Notification.Center;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class NotificationCenterPushRequestHandler : IAsyncQueueHandler<NotificationCenterPushRequestContext>
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ILogger<NotificationCenterPushRequestHandler> _logger;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly ITaskCommentDao _taskCommentDao;
    private readonly ITaskDao _taskDao;

    public NotificationCenterPushRequestHandler(
        IDbSessionProvider sessionProvider,
        ILogger<NotificationCenterPushRequestHandler> logger,
        INotificationCenterService notificationCenterService,
        ITaskCommentDao taskCommentDao,
        ITaskDao taskDao
    )
    {
        _sessionProvider = sessionProvider;
        _logger = logger;
        _notificationCenterService = notificationCenterService;
        _taskCommentDao = taskCommentDao;
        _taskDao = taskDao;
    }

    public async Task HandleAsync(
        NotificationCenterPushRequestContext commandContext,
        CancellationToken cancellationToken = default
    )
    {
        var producedUser = await _sessionProvider.CurrentSession.GetAsync<UserEntity>(commandContext.ProducedUserId, cancellationToken);
        if (producedUser == null)
        {
            throw new DataValidationException("ProducedUser can not be null");
        }

        IEntity entity = null!;
        if (commandContext.TaskId.HasValue)
        {
            entity = (await _taskDao.GetById(taskId: commandContext.TaskId.Value))!;
        }
        else if (commandContext.TaskCommentId.HasValue)
        {
            entity = (await _taskCommentDao.GetById(taskCommentId: commandContext.TaskCommentId.Value))!;
        }

        if (entity != null)
        {
            await _notificationCenterService.Push(commandContext.Action, producedUser, entity);
        }
    }
}
