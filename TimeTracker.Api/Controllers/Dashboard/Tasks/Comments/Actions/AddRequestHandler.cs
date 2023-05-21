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
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskCommentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly ITaskDao _taskDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskCommentDao _taskCommentDao;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            ITaskDao taskDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ITaskListDao taskListDao,
            ITaskCommentDao taskCommentDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _taskDao = taskDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _taskListDao = taskListDao;
            _taskCommentDao = taskCommentDao;
        }
    
        public async Task<TaskCommentDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var task = await _taskDao.GetById(request.TaskId);
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
            return _mapper.Map<TaskCommentDto>(taskComment);
        }
    }
}
