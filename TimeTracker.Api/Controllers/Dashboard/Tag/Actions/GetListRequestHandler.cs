using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tag.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITagDao _tagDao;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITagDao tagDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _tagDao = tagDao;
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

            var tags = await _tagDao.GetList(workspace);
            return new GetListResponse(
                _mapper.Map<ICollection<TagDto>>(tags),
                tags.Count
            );
        }
    }
}
