using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using AutoMapper;
using TimeTracker.Api.Dto.RequestsAndResponses.Storage;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;

namespace TimeTracker.Api.Controllers.Dashboard.Storage.Actions
{
    public class GetFileHandler : IAsyncRequestHandler<GetFileRequest, FileResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IFileStorage _fileStorage;
        private readonly IFileStorageRelationshipService _fileStorageRelationshipService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetFileHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IFileStorage fileStorage,
            IFileStorageRelationshipService fileStorageRelationshipService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _fileStorage = fileStorage;
            _fileStorageRelationshipService = fileStorageRelationshipService;
            _httpContextAccessor = httpContextAccessor;
        }
    
        public async Task<FileResponse> ExecuteAsync(GetFileRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();

            var (file, fileStream, mimeType) = await _fileStorage.GetFileStream(user, request.FileId, request.ImageSize);
            SetUpImageFileHeaders(file);
            fileStream.PrepareToCopy();
            return new FileResponse(fileStream, mimeType);
        }
        
        private void SetUpImageFileHeaders(StoredFileEntity file)
        {
            _httpContextAccessor.HttpContext?.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
            var lastModifiedTime = file.UpdatedAt ?? file.CreatedAt;
            _httpContextAccessor.HttpContext?.Response.Headers["Last-Modified"] = lastModifiedTime.ToString("R");
        }
    }
}
