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
    
    public string OriginalFileName { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string Url { get; set; } = string.Empty;
    
    public string ThumbUrl { get; set; } = string.Empty;
    
    public string Name => string.IsNullOrEmpty(Title) ? OriginalFileName : Title;
}
