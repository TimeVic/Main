using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class UploadHandler : IAsyncRequestHandler<UploadRequest, StoredFileDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IFileStorage _fileStorage;
        private readonly IFileStorageRelationshipService _fileStorageRelationshipService;

        public UploadHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IFileStorage fileStorage,
            IFileStorageRelationshipService fileStorageRelationshipService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _fileStorage = fileStorage;
            _fileStorageRelationshipService = fileStorageRelationshipService;
        }
    
        public async Task<StoredFileDto> ExecuteAsync(UploadRequest request)
        {
            if (request.File == null)
            {
                throw new IncorrectFileException("File was not provided");
            }
            
            var user = await _apiRequestService.GetCurrentUser();

            var entity = await _fileStorageRelationshipService.GetFileRelationship(
                request.EntityId,
                request.EntityType
            );
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, entity))
            {
                throw new HasNoAccessException();
            }
            var file = await _fileStorage.PutFileAsync(
                entity,
                request.File,
                request.FileType
            );
            return _mapper.Map<StoredFileDto>(file);
        }
    }
}
