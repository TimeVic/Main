using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.List.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskListDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IProjectDao _projectDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IProjectDao projectDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _projectDao = projectDao;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
        }
    
        public async Task<TaskListDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var project = await _projectDao.GetById(request.ProjectId, true);
            var taskList = await _taskListDao.GetById(request.TaskListId);
            RecordNotFoundException.ThrowIfNull(project);
            RecordNotFoundException.ThrowIfNull(taskList);
            
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, project))
            {
                throw new HasNoAccessException();
            }

            taskList.Project = project;
            taskList.Name = request.Name;
            return _mapper.Map<TaskListDto>(taskList);
        }
    }
}
