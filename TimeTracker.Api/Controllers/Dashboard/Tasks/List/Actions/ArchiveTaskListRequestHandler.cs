using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.List.Actions
{
    public class ArchiveTaskListRequestHandler : IAsyncRequestHandler<ArchiveTaskListRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;

        public ArchiveTaskListRequestHandler(
            IApiRequestService apiRequestService,
            ISecurityManager securityManager,
            ITaskListDao taskListDao
        )
        {
            _apiRequestService = apiRequestService;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
        }
    
        public async Task ExecuteAsync(ArchiveTaskListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var taskList = await _taskListDao.GetById(request.TaskListId);
            RecordNotFoundException.ThrowIfNull(taskList);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, taskList))
            {
                throw new HasNoAccessException();
            }
            await _taskListDao.ArchiveTaskListAsync(taskList);
        }
    }
}
