using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class GetForCalendarRequestHandler : IAsyncRequestHandler<GetForCalendarRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ITaskDao _taskDao;

        public GetForCalendarRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
            _taskDao = taskDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetForCalendarRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetByIdAsync(request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var taskLists = await _taskDao.GetList(workspace: workspace, filter: new GetTasksFilterDto
            {
                AssignedUserId = user.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            });
            return new GetListResponse(
                _mapper.Map<ICollection<TaskDto>>(taskLists.Items),
                taskLists.TotalCount
            );
        }
    }
}
