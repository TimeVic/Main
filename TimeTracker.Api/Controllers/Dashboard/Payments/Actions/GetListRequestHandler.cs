using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.Payments.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.GetWorkspaceById(request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException(nameof(request.WorkspaceId));
            }

            var listDto = await _projectDao.GetListAsync(workspace);
            return new GetListResponse(
                new List<PaymentDto>(),// _mapper.Map<ICollection<ProjectDto>>(listDto.Items),
                listDto.TotalCount
            );
        }
    }
}
