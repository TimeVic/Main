using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AutoMapper;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class GetFileHandler : IAsyncRequestHandler<GetFileRequest, FileResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IFileStorage _fileStorage;
        private readonly IFileStorageRelationshipService _fileStorageRelationshipService;

        public GetFileHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IFileStorage fileStorage,
            IFileStorageRelationshipService fileStorageRelationshipService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _fileStorage = fileStorage;
            _fileStorageRelationshipService = fileStorageRelationshipService;
        }
    
        public async Task<FileResponse> ExecuteAsync(GetFileRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);

            var (file, fileStream) = await _fileStorage.GetFileStream(user, request.FileId);
            fileStream.PrepareToCopy();
            return new FileResponse(fileStream, file.MimeType);
        }
    }
}
