using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceSettingsJiraDto : IResponse
{
    public virtual string? Url { get; set; } = "";
    
    public virtual string? ApiKey { get; set; } = "";
    
    public virtual string? UserName { get; set; } = "";
    
    public virtual bool IsActive { get; set; }
}
