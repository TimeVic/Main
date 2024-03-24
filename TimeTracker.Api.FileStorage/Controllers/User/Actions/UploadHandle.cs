using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;

namespace TimeTracker.Api.FileStorage.Controllers.User.Actions;

public class UploadHandle : IAsyncRequestHandler<UploadRequest, UploadResponse>
{
    public Task<UploadResponse> ExecuteAsync(UploadRequest request)
    {
        return new UploadResponse()
        {
            
        };
    }
}
