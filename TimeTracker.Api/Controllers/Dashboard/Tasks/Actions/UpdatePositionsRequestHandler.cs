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
    public class UpdatePositionsRequestHandler : IAsyncRequestHandler<UpdatePositionsRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;

        public UpdatePositionsRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
            _taskDao = taskDao;
        }
    
        public async Task ExecuteAsync(UpdatePositionsRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            
            var taskList = await _taskListDao.GetById(request.TaskListId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, taskList))
                throw new HasNoAccessException("This user has no permissions for provided task list");

            await _taskDao.UpdatePositions(taskList.Project.Workspace, request.Items);
        }
    }
}
