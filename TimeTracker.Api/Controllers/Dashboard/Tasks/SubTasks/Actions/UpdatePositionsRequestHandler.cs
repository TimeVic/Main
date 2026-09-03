using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.SubTasks.Actions
{
    public class UpdatePositionsRequestHandler : IAsyncRequestHandler<UpdatePositionsRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly ITaskDao _taskDao;
        private readonly ITaskSubTaskDao _taskSubTaskDao;
        private readonly ISecurityManager _securityManager;

        public UpdatePositionsRequestHandler(
            IApiRequestService apiRequestService,
            ITaskDao taskDao,
            ITaskSubTaskDao taskSubTaskDao,
            ISecurityManager securityManager
        )
        {
            _apiRequestService = apiRequestService;
            _taskDao = taskDao;
            _taskSubTaskDao = taskSubTaskDao;
            _securityManager = securityManager;
        }

        public async Task ExecuteAsync(UpdatePositionsRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var task = await _taskDao.GetById(request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, task))
            {
                throw new HasNoAccessException();
            }

            await _taskSubTaskDao.UpdatePositionsAsync(task!, request.Positions);
        }
    }
}
