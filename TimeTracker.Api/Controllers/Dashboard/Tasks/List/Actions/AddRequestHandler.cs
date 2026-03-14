using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.List.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskListDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskListDao _taskListDao;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ITaskListDao taskListDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _taskListDao = taskListDao;
        }
    
        public async Task<TaskListDto> ExecuteAsync(AddRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var project = await _projectDao.GetById(request.ProjectId, true);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }
            var taskList = await _taskListDao.CreateTaskListAsync(project, request.Name);
            return _mapper.Map<TaskListDto>(taskList);
        }
    }
}
