using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Business.FileStorage.Mvc.Attribute;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;

public class UploadRequest: IRequest<FileStorageFileDto>
{
    [Required]
    [IsStorageBucketName]
    public required string Bucket { get; set; }
        
    [Required]
    public required IFormFile File { get; set; }
    
    [StringLength(512)]
    public string? Directory { get; set; }
}
