using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskFullDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
            _taskDao = taskDao;
        }
    
        public async Task<TaskFullDto> ExecuteAsync(UpdateRequest request)
        {
            var currentUser = await _apiRequestService.GetCurrentUser();
            var assignee = await _userDao.GetById(request.UserId);
            RecordNotFoundException.ThrowIfNull(assignee, "User not found");
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (taskList == null)
            {
                throw new ValidationException("Incorrect TaskListId");
            }
            
            var task = await _taskDao.GetById(request.TaskId);
            RecordNotFoundException.ThrowIfNull(task);
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, currentUser, taskList))
                throw new HasNoAccessException("This user has no permissions for provided task list");
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, currentUser, task))
                throw new HasNoAccessException("This user has no permissions for task");
            if (taskList.Project.Client.Workspace.Id != task.Workspace.Id)
                throw new ValidationException("Incorrect TaskListId");

            // Workspace members may be assigned even when they do not have access to this project.
            if (!await _securityManager.HasAccess(AccessLevel.Read, assignee, task.Workspace))
                throw new HasNoAccessException("This user has no permissions for task workspace");

            if (task.ExtendedStatus == ExtendedTaskStatus.InProgress && request.Status != task.Status)
            {
                throw new ValidationException("Cannot change status of a task with an active timer");
            }
            
            task = _mapper.Map(request, task);
            var tags = task.Workspace.Tags.Where(
                item => request.TagIds.Any(tagId => item.Id == tagId)
            );
            task = await _taskDao.UpdateTaskAsync(
                task,
                taskList: taskList,
                user: assignee,
                title: request.Title!,
                description: request.Description,
                originalEstimate: request.OriginalEstimate ?? task.OriginalEstimate,
                startTime: request.StartTime,
                endTime: request.EndTime,
                status: request.Status,
                priority: request.Priority,
                isArchived: request.IsArchived,
                tags: tags,
                reminderTime: request.ReminderTime
            );
            var result = _mapper.Map<TaskFullDto>(task);
            var trackedDurationMap = await _taskDao.GetTrackedDurationByTaskIds(new[] { task.Id });
            result.TrackedDuration = trackedDurationMap.TryGetValue(task.Id, out var trackedDuration)
                ? trackedDuration
                : TimeSpan.Zero;
            return result;
        }
    }
}
