using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class SetRequestHandler : IAsyncRequestHandler<SetRequest, TimeEntryDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;

        public SetRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
        }
    
        public async Task<TimeEntryDto> ExecuteAsync(SetRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.GetWorkspaceById(request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException(nameof(request.WorkspaceId));
            }

            var timeEntry = await _timeEntryDao.GetByIdAsync(request.Id);
            if (timeEntry != null && !await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
            {
                throw new HasNoAccessException();
            }

            var userProjects = await _projectDao.GetByUser(user);
            timeEntry = await _timeEntryDao.SetAsync(
                workspace,
                new TimeEntryCreationDto()
                {
                    Id = timeEntry?.Id,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    HourlyRate = request.HourlyRate,
                    IsBillable = request.IsBillable
                },
                userProjects.FirstOrDefault(item => item.Id == request.ProjectId)
            );
            return _mapper.Map<TimeEntryDto>(timeEntry);
        }
    }
}
