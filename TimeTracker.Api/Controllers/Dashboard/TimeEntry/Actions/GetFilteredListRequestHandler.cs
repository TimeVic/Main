using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto;
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
    public class GetFilteredListRequestHandler : IAsyncRequestHandler<GetFilteredListRequest, GetFilteredListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public GetFilteredListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<GetFilteredListResponse> ExecuteAsync(GetFilteredListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var listDto = await _timeEntryDao.GetListAsync(
                workspace, 
                request.Page, 
                filter: new FilterDataDto
                {
                    Search = request.Search,
                    ClientId = request.ClientId,
                    IsBillable = request.IsBillable,
                    ProjectId = request.ProjectId,
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
