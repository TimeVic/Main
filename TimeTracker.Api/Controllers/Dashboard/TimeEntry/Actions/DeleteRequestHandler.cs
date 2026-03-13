using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITimeEntryService _timeEntryService;

        public DeleteRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager,
            ITimeEntryService timeEntryService
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
            _timeEntryService = timeEntryService;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
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

            await _timeEntryService.DeleteAsync(timeEntry);
        }
    }
}
