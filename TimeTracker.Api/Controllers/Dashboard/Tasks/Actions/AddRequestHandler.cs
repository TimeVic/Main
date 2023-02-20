using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Task;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ITaskListDao taskListDao,
            ITaskDao taskDao
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
            _taskDao = taskDao;
        }
    
        public async Task<TaskDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, taskList.Project))
            {
                throw new HasNoAccessException();
            }
            
            var task = await _taskDao.AddTaskAsync(
                taskList,
                user,
                request.Title,
                request.Description,
                request.NotificationTime
            );
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<TaskDto>(task);
        }
    }
}
