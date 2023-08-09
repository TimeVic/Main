using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Comments.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITaskDao _taskDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskCommentDao _taskCommentDao;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITaskDao taskDao,
            ISecurityManager securityManager,
            ITaskCommentDao taskCommentDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _taskDao = taskDao;
            _securityManager = securityManager;
            _taskCommentDao = taskCommentDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var task = await _taskDao.GetByWorkspaceTaskId(request.WorkspaceId, request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
            {
                throw new HasNoAccessException();
            }

            var list = await _taskCommentDao.GetList(task, request.Page);
            return new GetListResponse(
                _mapper.Map<ICollection<TaskCommentDto>>(list.Items),
                list.TotalCount
            );
        }
    }
}
