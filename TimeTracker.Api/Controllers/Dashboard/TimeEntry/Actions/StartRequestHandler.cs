using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StartRequestHandler : IAsyncRequestHandler<StartRequest, TimeEntryDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ITimeEntryDao _timeEntryDao;

        public StartRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ITimeEntryDao timeEntryDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _timeEntryDao = timeEntryDao;
        }
    
        public async Task<TimeEntryDto> ExecuteAsync(StartRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.GetWorkspaceById(request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException("Workspace not found");
            }

            var timeEntry = await _timeEntryDao.StartNewAsync(
                user,
                workspace,
                request.IsBillable,
                request.Description,
                request.ProjectId
            );
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<TimeEntryDto>(timeEntry);
        }
    }
}
