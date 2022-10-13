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
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var listDto = await _timeEntryDao.GetListAsync(workspace, request.Page, new FilterDataDto()
            {
                UserId = user.Id
            });
            var activeTimeEntry = await _timeEntryDao.GetActiveEntryAsync(workspace, user);
            return new GetListResponse
            {
                List = new PaginatedListDto<TimeEntryDto>(
                    _mapper.Map<ICollection<TimeEntryDto>>(listDto.Items),
                    listDto.TotalCount
                ),
                ActiveTimeEntry = _mapper.Map<TimeEntryDto>(activeTimeEntry)
            };
        }
    }
}
