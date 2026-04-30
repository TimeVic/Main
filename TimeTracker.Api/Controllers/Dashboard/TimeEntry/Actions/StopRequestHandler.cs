using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StopRequestHandler : IAsyncRequestHandler<StopRequest, TimeEntryDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryService _timeEntryService;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskDao _taskDao;
        private readonly IDbSessionProvider _sessionProvider;

        public StopRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryService timeEntryService,
            ISecurityManager securityManager,
            ITaskDao taskDao,
            IDbSessionProvider sessionProvider
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _timeEntryService = timeEntryService;
            _securityManager = securityManager;
            _taskDao = taskDao;
            _sessionProvider = sessionProvider;
        }
    
        public async Task<TimeEntryDto> ExecuteAsync(StopRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var timeEntries = await _timeEntryService.StopActiveAsync(
                workspace!,
                user,
                request.EndTime
            );

            var timeEntry = timeEntries.FirstOrDefault();
            if (timeEntry == null)
            {
                return new TimeEntryDto();
            }

            await _sessionProvider.CurrentSession.FlushAsync();

            var result = _mapper.Map<TimeEntryDto>(timeEntry);
            if (result.Task != null)
            {
                var trackedDurationMap = await _taskDao.GetTrackedDurationByTaskIds(new[] { result.Task.Id });
                result.Task.TrackedDuration = trackedDurationMap.TryGetValue(result.Task.Id, out var trackedDuration)
                    ? trackedDuration
                    : TimeSpan.Zero;
            }

            return result;
        }
    }
}
