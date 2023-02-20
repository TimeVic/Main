using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tag.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TagDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITagDao _tagDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            ITagDao tagDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _tagDao = tagDao;
        }
    
        public async Task<TagDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            var tag = await _tagDao.GetById(request.TagId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, tag.Workspace))
            {
                throw new HasNoAccessException();
            }
            tag = _mapper.Map(request, tag);
            await _sessionProvider.CurrentSession.SaveAsync(tag);
            
            return _mapper.Map<TagDto>(tag);
        }
    }
}
