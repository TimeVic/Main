using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.FileStorage.Services.Api;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Dao.FileStorage;

namespace TimeTracker.Api.FileStorage.Controllers.User.Actions;

public class UploadHandle : IAsyncRequestHandler<UploadRequest, FileStorageFileDto>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageBucketDao _storageBucketDao;
    private readonly IFileStorageSecurityService _storageSecurityService;
    private readonly IMapper _mapper;

    public UploadHandle(
        IFileStorageService fileStorageService,
        IFileStorageBucketDao storageBucketDao,
        IFileStorageSecurityService storageSecurityService,
        IMapper mapper
    )
    {
        _fileStorageService = fileStorageService;
        _storageBucketDao = storageBucketDao;
        _storageSecurityService = storageSecurityService;
        _mapper = mapper;
    }

    public async Task<FileStorageFileDto> ExecuteAsync(UploadRequest request)
    {
        var user = await _storageSecurityService.GetCurrentUser();
        var bucket = await _storageBucketDao.GetByName(user, request.Bucket);
        if (bucket == null)
        {
            throw new DataValidationException("Bucket was not found or not available for this user");
        }
        var addedFile = await _fileStorageService.Put(bucket, request.File, request.Directory);
        return _mapper.Map<FileStorageFileDto>(addedFile);
    }
}
