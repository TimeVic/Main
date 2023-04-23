using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.ExternalClients;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, TaskDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;
        private readonly IClickUpClient _clickUpClient;
        private readonly ITimeEntryDao _timeEntryDao;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao,
            IClickUpClient clickUpClient,
            ITimeEntryDao timeEntryDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
            _taskDao = taskDao;
            _clickUpClient = clickUpClient;
            _timeEntryDao = timeEntryDao;
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

            TaskEntity task = null;
            if (!string.IsNullOrEmpty(request.ExternalTaskId))
            {
                task = await CreateFromExternalId(request, taskList, user);
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
                    request.NotificationTime
                );
                if (request.TimeEntryId != null)
                {
                    var timeEntry = await GetTimeEntry(request.TimeEntryId.Value, user);
                    timeEntry.Task = task;
                }
            }
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<TaskDto>(task);
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
                    return await _clickUpClient.SetTimeEntryTaskAsync(
                        timeEntry,
                        taskList,
                        request.ExternalTaskId
                    );
                }
                return await _clickUpClient.SetTimeEntryTaskAsync(
                    taskList,
                    user,
                    request.ExternalTaskId
                );
            }

            return null;
        }

        private async Task<TimeEntryEntity> GetTimeEntry(long timeEntryId, UserEntity user)
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
