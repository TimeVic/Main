using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.FileStorage.Mvc.Attribute;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.Storage;

public class UploadResponse: IResponse
{
    public string BucketName { get; set; } = string.Empty;
    
    public string FileName { get; set; } = string.Empty;
        
    public string MimeType { get; set; } = string.Empty;
    
    public long Size { get; set; }
}
