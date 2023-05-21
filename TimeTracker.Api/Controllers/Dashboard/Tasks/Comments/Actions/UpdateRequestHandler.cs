using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Comments.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskCommentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITaskDao _taskDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskCommentDao _taskCommentDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITaskDao taskDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            ITaskCommentDao taskCommentDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _taskDao = taskDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _taskCommentDao = taskCommentDao;
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
            return _mapper.Map<TaskCommentDto>(taskComment);
        }
    }
}
