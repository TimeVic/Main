using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class WorkspaceSettingsClickUpDto : IResponse
{
    public virtual string SecurityKey { get; set; } = "";
    
    public virtual string TeamId { get; set; } = "";

    public virtual bool IsCustomTaskIds { get; set; }
    
    public virtual bool IsFillTimeEntryWithTaskDetails { get; set; }
    
    public virtual bool IsActive { get; set; }
}
