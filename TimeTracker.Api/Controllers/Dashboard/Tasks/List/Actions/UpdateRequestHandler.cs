using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Task;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.List.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskListDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskListDao _taskListDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ITaskListDao taskListDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _taskListDao = taskListDao;
        }
    
        public async Task<TaskListDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var project = await _projectDao.GetById(request.ProjectId);
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (taskList == null || project == null)
            {
                throw new RecordNotFoundException();
            }
            
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project.Workspace))
            {
                throw new HasNoAccessException();
            }

            taskList.Name = request.Name;
            return _mapper.Map<TaskListDto>(taskList);
        }
    }
}
