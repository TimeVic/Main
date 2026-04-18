using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceSettingsRedmineDto : IResponse
{
    public virtual string Url { get; set; } = string.Empty;
    
    public virtual string ApiKey { get; set; } = string.Empty;
    
    public virtual long RedmineUserId { get; set; }
    
    public virtual long ActivityId { get; set; }
    
    public virtual bool IsActive { get; set; }
}
