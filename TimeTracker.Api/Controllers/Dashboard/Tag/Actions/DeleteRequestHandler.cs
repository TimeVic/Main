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
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly ITagDao _tagDao;

        public DeleteRequestHandler(
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
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            var tag = await _tagDao.GetById(request.TagId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, tag.Workspace))
            {
                throw new HasNoAccessException();
            }

            await _tagDao.DeleteTag(tag);
        }
    }
}
