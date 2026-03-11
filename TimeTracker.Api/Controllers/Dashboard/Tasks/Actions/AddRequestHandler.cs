using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskFullDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;
        private readonly IClickUpClient _clickUpClient;
        private readonly IJiraClient _jiraClient;
        private readonly ITimeEntryDao _timeEntryDao;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao,
            IClickUpClient clickUpClient,
            IJiraClient jiraClient,
            ITimeEntryDao timeEntryDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
            _taskDao = taskDao;
            _clickUpClient = clickUpClient;
            _jiraClient = jiraClient;
            _timeEntryDao = timeEntryDao;
        }
    
        public async Task<TaskFullDto> ExecuteAsync(AddRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            RecordNotFoundException.ThrowIfNull(user);
            var taskList = await _taskListDao.GetById(request.TaskListId);
            RecordNotFoundException.ThrowIfNull(taskList);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, taskList.Project))
            {
                throw new HasNoAccessException();
            }

            TaskEntity task = null;
            if (!string.IsNullOrEmpty(request.ExternalTaskId))
            {
                task = await CreateFromExternalId(request, taskList, user);
                task = await _taskDao.UpdateTaskAsync(
                    task,
                    taskList: taskList,
                    user: user,
                    title: task.Title,
                    description: task.Description,
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    status: request.Status,
                    priority: request.Priority,
                    isAddHistoryItem: false
                );
            }
            if (task == null)
            {
                if (string.IsNullOrEmpty(request.Title))
                {
                    throw new ValidationException("Title is required");
                }

                task = await _taskDao.AddTaskAsync(
                    taskList,
                    user,
                    request.Title,
                    request.Description,
                    startTime: request.StartTime,
                    endTime: request.EndTime,
                    status: request.Status,
                    priority: request.Priority
                );
                if (request.TimeEntryId != null)
                {
                    var timeEntry = await GetTimeEntry(request.TimeEntryId.Value, user);
                    timeEntry.Task = task;
                    if (timeEntry.Project == null)
                    {
                        timeEntry.Project = taskList.Project;
                    }
                }
            }
            return _mapper.Map<TaskFullDto>(task);
        }

        private async Task<TaskEntity> CreateFromExternalId(
            AddRequest request,
            TaskListEntity taskList,
            UserEntity user
        )
        {
            if (_clickUpClient.IsCorrectTaskId(request.ExternalTaskId))
            {
                if (request.TimeEntryId != null)
                {
                    var timeEntry = await GetTimeEntry(request.TimeEntryId.Value, user);
                    if (!await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
                    {
                        throw new HasNoAccessException("Has no access to TimeEntry");
                    }

                    try
                    {
                        return await _jiraClient.SetTimeEntryTaskAsync(
                            timeEntry,
                            taskList,
                            request.ExternalTaskId
                        );
                    }
                    catch (Exception) {}
                    return await _clickUpClient.SetTimeEntryTaskAsync(
                        timeEntry,
                        taskList,
                        request.ExternalTaskId
                    );
                }

                try
                {
                    return await _jiraClient.SetTimeEntryTaskAsync(
                        taskList,
                        user,
                        request.ExternalTaskId
                    );
                }
                catch (Exception) {}
                return await _clickUpClient.SetTimeEntryTaskAsync(
                    taskList,
                    user,
                    request.ExternalTaskId
                );
            }

            return null;
        }

        private async Task<TimeEntryEntity> GetTimeEntry(Guid timeEntryId, UserEntity user)
        {
            var timeEntry = await _sessionProvider.CurrentSession
                .GetAsync<TimeEntryEntity>(timeEntryId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
            {
                throw new HasNoAccessException("Has no access to TimeEntry");
            }

            return timeEntry;
        }
    }
}
