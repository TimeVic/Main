using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Notifications.Senders.Tasks.Comments;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Comments.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskCommentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITaskDao _taskDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskCommentDao _taskCommentDao;
        private readonly IQueueService _queueService;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITaskDao taskDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            ITaskCommentDao taskCommentDao,
            IQueueService queueService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _taskDao = taskDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _taskCommentDao = taskCommentDao;
            _queueService = queueService;
        }
    
        public async Task<TaskCommentDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var task = await _taskDao.GetByWorkspaceTaskId(request.WorkspaceId, request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, task))
            {
                throw new HasNoAccessException();
            }

            var watchers = new List<UserEntity>();
            if (request.WatcherIds != null)
            {
                foreach (var watcherId in request.WatcherIds)
                {
                    var watchUser = await _userDao.GetById(watcherId);
                    if (!await _securityManager.HasAccess(AccessLevel.Read, watchUser, task))
                    {
                        throw new HasNoAccessException();
                    }
                    watchers.Add(watchUser);
                }
            }
            var taskComment = await _taskCommentDao.AddAsync(
                task,
                user,
                request.Comment,
                watchers
            );
            await _sessionProvider.PerformCommitAsync();
            await SendNotification(taskComment);
            return _mapper.Map<TaskCommentDto>(taskComment);
        }

        private async Task SendNotification(TaskCommentEntity comment)
        {
            await _queueService.PushDefaultAsync(new NotificationCenterPushRequestContext()
            {
                Action = NotificationActionType.AddEntity,
                TaskCommentId = comment.Id,
                ProducedUserId = comment.User.Id
            });
            
            var receivers = new List<UserEntity>();
            receivers.Add(comment.User);
            receivers = receivers.Concat(comment.Watchers).ToList();
            receivers = receivers.DistinctBy(item => item.Email).ToList();
            foreach (var receiver in receivers)
            {
                await _queueService.PushNotificationAsync(new SetCommentNotificationContext()
                {
                    ToAddress = receiver.Email,
                    Comment = comment.Comment,
                    TaskId = comment.Task.TaskId,
                    WorkspaceId = comment.Task.TaskList.Project.Workspace.Id,
                    IsUpdated = false,
                    OwnerName = comment.User.Name
                });
            }
        }
    }
}
