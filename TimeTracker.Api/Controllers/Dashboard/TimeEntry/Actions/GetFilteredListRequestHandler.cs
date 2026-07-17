using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class GetFilteredListRequestHandler : IAsyncRequestHandler<GetFilteredListRequest, GetFilteredListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskDao _taskDao;

        public GetFilteredListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _taskDao = taskDao;
        }
    
        public async Task<GetFilteredListResponse> ExecuteAsync(GetFilteredListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            if (request.TaskId.HasValue)
            {
                var task = await _taskDao.GetById(request.TaskId.Value);
                RecordNotFoundException.ThrowIfNull(task);
                if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                {
                    throw new HasNoAccessException();
                }
            }

            var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace!);
            var listDto = await _timeEntryDao.GetListAsync(
                workspace!, 
                request.Page, 
                filter: new FilterDataDto
                {
                    Search = request.Search,
                    ClientId = request.ClientId,
                    IsBillable = request.IsBillable,
                    ProjectId = request.ProjectId,
                    TaskId = request.TaskId,
                    MemberId = request.MemberId,
                    DateFrom = request.DateFrom,
                    DateTo = request.DateTo
                },
                user: user,
                accessType: userAccess.Value
            );
            return new GetFilteredListResponse(
                _mapper.Map<ICollection<TimeEntryDto>>(listDto.Items),
                listDto.TotalCount
            );
        }
    }
}
