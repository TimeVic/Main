using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _userDao = userDao;
            _securityManager = securityManager;
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
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (taskList == null)
            {
                throw new ValidationException("Incorrect TaskListId");
            }
            
            var task = await _taskDao.GetByWorkspaceTaskId(
                taskList.Project.Workspace.Id,
                request.TaskId
            );
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, taskList))
                throw new HasNoAccessException("This user has no permissions for provided task list");
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                throw new HasNoAccessException("This user has no permissions for task");
            if (taskList.Project.Workspace != task.Workspace)
                throw new ValidationException("Incorrect TaskListId");
            
            task = _mapper.Map(request, task);
            var tags = task.Workspace.Tags.Where(
                item => request.TagIds.Any(tagId => item.Id == tagId)
            );
            await _taskDao.UpdateTaskAsync(
                task,
                taskList: taskList,
                user: user,
                title: request.Title,
                description: request.Description,
                startTime: request.StartTime,
                endTime: request.EndTime,
                status: request.Status,
                priority: request.Priority,
                isArchived: request.IsArchived,
                tags: tags,
                reminderTime: request.ReminderTime
            );
            return _mapper.Map<TaskDto>(task);
        }
    }
}
