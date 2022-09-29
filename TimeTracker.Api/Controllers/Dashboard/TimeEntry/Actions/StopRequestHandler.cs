using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.TimeEntry;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StopRequestHandler : IAsyncRequestHandler<StopRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryService _timeEntryService;

        public StopRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ITimeEntryService timeEntryService
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _timeEntryService = timeEntryService;
        }
    
        public async Task ExecuteAsync(StopRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.GetWorkspaceById(request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException("Workspace not found");
            }

            await _timeEntryService.StopActiveAsync(workspace);
        }
    }
}
