using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.File;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Dao.FileStorage;

namespace TimeTracker.Api.FileStorage.Controllers.File.Actions;

public class GetHandle : IAsyncRequestHandler<GetRequest, GetResponse>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageFileDao _storageFileDao;

    public GetHandle(
        IFileStorageService fileStorageService,
        IFileStorageFileDao storageFileDao
    )
    {
        _fileStorageService = fileStorageService;
        _storageFileDao = storageFileDao;
    }

    public async Task<GetResponse> ExecuteAsync(GetRequest request)
    {
        // var user = await _storageSecurityService.GetCurrentUser();
        var file = await _storageFileDao.GetByExternalId(request.Bucket, request.FileId);
        if (file == null)
        {
            throw new DataValidationException("Bucket was not found or not available for this user");
        }
        var fileStream = await _fileStorageService.DownloadToStream(file);
        fileStream.PrepareToCopy();
        return new GetResponse(fileStream, file.MimeType);
    }
}
