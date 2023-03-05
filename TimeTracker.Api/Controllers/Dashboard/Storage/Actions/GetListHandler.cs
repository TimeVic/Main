using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class GetListHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IFileStorage _fileStorage;
        private readonly IFileStorageRelationshipService _fileStorageRelationshipService;
        private readonly IDbSessionProvider _sessionProvider;

        public GetListHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IFileStorage fileStorage,
            IFileStorageRelationshipService fileStorageRelationshipService,
            IDbSessionProvider sessionProvider
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _fileStorage = fileStorage;
            _fileStorageRelationshipService = fileStorageRelationshipService;
            _sessionProvider = sessionProvider;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            if (request.EntityType == StorageEntityType.Task)
            {
                var task = await _sessionProvider.CurrentSession.GetAsync<TaskEntity>(request.EntityId);
                if (task == null)
                {
                    throw new RecordNotFoundException();
                }
                if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                {
                    throw new HasNoAccessException();
                }
                return new GetListResponse(
                    _mapper.Map<ICollection<StoredFileDto>>(task.Attachments),
                    task.Attachments.Count
                );
            }
            throw new ValidationException("Incorrect entity type");
        }
    }
}
