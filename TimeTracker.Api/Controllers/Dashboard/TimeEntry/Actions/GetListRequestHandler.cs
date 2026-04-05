using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (workspace == null)
            {
                throw new HasNoAccessException();
            }
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var listDto = await _timeEntryDao.GetListGroupedByDayAsync(workspace, user, request.Page);
            var activeTimeEntry = await _timeEntryDao.GetActiveEntryAsync(workspace, user);
            return new GetListResponse
            {
                List = new PaginatedListDto<TimeEntryDto>(
                    _mapper.Map<ICollection<TimeEntryDto>>(listDto.Items),
                    listDto.TotalCount,
                    GlobalConstants.TimeEntryGroupedByDayPageSize
                ),
                ActiveTimeEntry = _mapper.Map<TimeEntryDto>(activeTimeEntry)
            };
        }
    }
}
