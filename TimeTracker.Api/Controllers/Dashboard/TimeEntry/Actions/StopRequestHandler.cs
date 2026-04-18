using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StopRequestHandler : IAsyncRequestHandler<StopRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryService _timeEntryService;
        private readonly ISecurityManager _securityManager;

        public StopRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryService timeEntryService,
            ISecurityManager securityManager
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _timeEntryService = timeEntryService;
            _securityManager = securityManager;
        }
    
        public async Task ExecuteAsync(StopRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            await _timeEntryService.StopActiveAsync(
                workspace!,
                user,
                request.EndTime
            );
        }
    }
}
