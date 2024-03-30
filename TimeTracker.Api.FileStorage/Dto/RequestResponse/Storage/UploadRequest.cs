using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.FileStorage.Mvc.Attribute;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;

public class UploadRequest: IRequest<UploadResponse>
{
    [Required]
    [IsStorageBucketName]
    public required string BucketName { get; set; }
        
    [Required]
    public required IFormFile File { get; set; }
}
