using Api.Requests.Abstractions;
using AutoMapper;
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
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskCommentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskCommentDao _taskCommentDao;
        private readonly IQueueService _queueService;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskCommentDao taskCommentDao,
            IQueueService queueService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskCommentDao = taskCommentDao;
            _queueService = queueService;
        }
    
        public async Task<TaskCommentDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var taskComment = await _taskCommentDao.GetById(request.CommentId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, taskComment))
            {
                throw new HasNoAccessException();
            }

            var watchers = new List<UserEntity>();
            if (request.WatcherIds != null)
            {
                foreach (var watcherId in request.WatcherIds)
                {
                    var watchUser = await _userDao.GetById(watcherId);
                    if (!await _securityManager.HasAccess(AccessLevel.Read, watchUser, taskComment))
                    {
                        throw new HasNoAccessException();
                    }
                    watchers.Add(watchUser);
                }
            }
            taskComment = await _taskCommentDao.UpdateAsync(
                taskComment,
                request.Comment,
                watchers
            );
            await SendNotification(taskComment, user);
            return _mapper.Map<TaskCommentDto>(taskComment);
        }
        
        private async Task SendNotification(TaskCommentEntity comment, UserEntity producedUser)
        {
            await _queueService.PushDefaultAsync(new NotificationCenterPushRequestContext()
            {
                Action = NotificationActionType.EditEntity,
                TaskCommentId = comment.Id,
                ProducedUserId = producedUser.Id
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
                    IsUpdated = true,
                    OwnerName = comment.User.Name
                });
            }
        }
    }
}
