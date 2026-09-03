using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.SubTasks.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskSubTaskDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly ITaskDao _taskDao;
        private readonly ITaskSubTaskDao _taskSubTaskDao;
        private readonly ISecurityManager _securityManager;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            ITaskDao taskDao,
            ITaskSubTaskDao taskSubTaskDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _taskDao = taskDao;
            _taskSubTaskDao = taskSubTaskDao;
            _securityManager = securityManager;
        }

        public async Task<TaskSubTaskDto> ExecuteAsync(AddRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var task = await _taskDao.GetById(request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, task))
            {
                throw new HasNoAccessException();
            }

            if (task!.SubTasksCount >= GlobalConstants.MaxSubTasksPerTask || task.SubTasks.Count >= GlobalConstants.MaxSubTasksPerTask)
            {
                throw new DataValidationException($"Task cannot have more than {GlobalConstants.MaxSubTasksPerTask} subtasks.");
            }

            var subTask = await _taskSubTaskDao.AddAsync(task!, request.Title);
            return _mapper.Map<TaskSubTaskDto>(subTask);
        }
    }
}
