using System.ComponentModel.DataAnnotations;
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
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskDto>
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

        public UpdateRequestHandler(
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
    
        public async Task<TaskDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _userDao.GetById(request.UserId);
            if (user == null)
            {
                throw new RecordNotFoundException("User not found");
            }

            var task = await _taskDao.GetById(request.TaskId);
            var newTaskList = await _taskListDao.GetById(request.TaskListId);
            if (newTaskList == null || newTaskList.Project.Workspace != task.Workspace)
            {
                throw new ValidationException("Incorrect TaskListId");
            }
            task = _mapper.Map(request, task);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, newTaskList))
                throw new HasNoAccessException("This user has no permissions for provided task list");
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                throw new HasNoAccessException("This user has no permissions for task");
            
            task.TaskList = newTaskList;
            task.User = user;
            return _mapper.Map<TaskDto>(task);
        }
    }
}
