using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class StoredFileDto: BaseDto
{
    public StoredFileType Type { get; set; }
    
    public StoredFileStatus Status { get; set; }
    
    public string? Extension { get; set; }
    
    public string? MimeType { get; set; }
    
    public string OriginalFileName { get; set; }
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string Url { get; set; }
    
    public string ThumbUrl { get; set; }
    
    public string Name => string.IsNullOrEmpty(Title) ? OriginalFileName : Title;
}
