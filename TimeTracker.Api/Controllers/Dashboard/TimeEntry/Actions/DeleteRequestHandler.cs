using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var timeEntry = await _timeEntryDao.GetByIdAsync(request.TimeEntryId);
            if (timeEntry == null)
            {
                throw new RecordNotFoundException();
            }
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
            {
                throw new PermissionException();
            }

            await _timeEntryDao.DeleteAsync(timeEntry);
        }
    }
}
