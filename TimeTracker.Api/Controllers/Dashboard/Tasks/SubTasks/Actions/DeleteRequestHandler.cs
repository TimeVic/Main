using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.SubTasks.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly ITaskSubTaskDao _taskSubTaskDao;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IApiRequestService apiRequestService,
            ITaskSubTaskDao taskSubTaskDao,
            ISecurityManager securityManager
        )
        {
            _apiRequestService = apiRequestService;
            _taskSubTaskDao = taskSubTaskDao;
            _securityManager = securityManager;
        }

        public async Task ExecuteAsync(DeleteRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var subTask = await _taskSubTaskDao.GetById(request.SubTaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, subTask))
            {
                throw new HasNoAccessException();
            }

            await _taskSubTaskDao.DeleteAsync(subTask!);
        }
    }
}
