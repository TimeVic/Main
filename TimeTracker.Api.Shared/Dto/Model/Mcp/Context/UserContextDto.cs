using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.Model.Mcp.Context;

public class UserContextDto
{
    public UserDto User { get; set; } = null!;
    
    public ICollection<WorkspaceDto> Workspaces { get; set; } = new List<WorkspaceDto>();
    
    public ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    
    public DateTime CurrentTimeUtc { get; set; }
}

