using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

public class UploadRequest: IRequest<StoredFileDto>
{
    [Required]
    public long EntityId { get; set; }
        
    [Required]
    public StorageEntityType EntityType { get; set; }
        
    [Required]
    public StoredFileType FileType { get; set; }
        
    [Required]
    public IFormFile File { get; set; }
}
