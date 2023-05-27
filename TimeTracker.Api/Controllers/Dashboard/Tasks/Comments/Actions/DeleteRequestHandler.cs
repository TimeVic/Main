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
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITaskDao _taskDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskCommentDao _taskCommentDao;

        public DeleteRequestHandler(
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
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var taskComment = await _taskCommentDao.GetById(request.CommentId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, taskComment))
            {
                throw new HasNoAccessException();
            }

            await _taskCommentDao.DeleteAsync(taskComment);
        }
    }
}
