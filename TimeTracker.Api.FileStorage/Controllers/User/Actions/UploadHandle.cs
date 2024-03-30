using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;
using TimeTracker.Business.FileStorage.Services.Storage;

namespace TimeTracker.Api.FileStorage.Controllers.User.Actions;

public class UploadHandle : IAsyncRequestHandler<UploadRequest, UploadResponse>
{
    private readonly IFileStorageService _fileStorageService;

    public UploadHandle(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public Task<UploadResponse> ExecuteAsync(UploadRequest request)
    {
        return new UploadResponse()
        {
            
        };
    }
}
