using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Directory;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.File;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.FileStorage.Services.Api;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Dao.FileStorage;

namespace TimeTracker.Api.FileStorage.Controllers.Directory.Actions;

public class GetFilesHandle: IAsyncRequestHandler<GetFilesRequest, PaginatedListDto<FileStorageFileDto>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageBucketDao _storageBucketDao;
    private readonly IFileStorageDirectoryManagerService _directoryManagerService;
    private readonly IFileStorageSecurityService _storageSecurityService;

    public GetFilesHandle(
        IFileStorageService fileStorageService,
        IFileStorageBucketDao storageBucketDao,
        IFileStorageDirectoryManagerService directoryManagerService,
        IFileStorageSecurityService storageSecurityService
    )
    {
        _fileStorageService = fileStorageService;
        _storageBucketDao = storageBucketDao;
        _directoryManagerService = directoryManagerService;
        _storageSecurityService = storageSecurityService;
    }

    public async Task<PaginatedListDto<FileStorageFileDto>> ExecuteAsync(GetFilesRequest request)
    {
        var user = await _storageSecurityService.GetCurrentUser();
        var bucket = await _storageBucketDao.GetByName(user, request.Bucket);
        if (bucket == null)
        {
            throw new DataValidationException("Bucket was not found or not available for this user");
        }
        
        // _directoryManagerService.CreateRecursive()
        //
        // var fileStream = await _fileStorageService.DownloadToStream(file);
        // fileStream.PrepareToCopy();
        // return new GetResponse(fileStream, file.MimeType);
        return new PaginatedListDto<FileStorageFileDto>();
    }
}
