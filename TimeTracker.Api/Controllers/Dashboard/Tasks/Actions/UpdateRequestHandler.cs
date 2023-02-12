﻿using Api.Requests.Abstractions;
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
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var task = await _taskDao.GetById(request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, task.Workspace))
            {
                throw new HasNoAccessException();
            }
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (taskList == null || taskList.Project.Workspace != task.Workspace)
            {
                throw new ValidationException("Incorrect TaskListId");
            }

            task = _mapper.Map(request, task);
            task.TaskList = taskList;
            return _mapper.Map<TaskDto>(task);
        }
    }
}