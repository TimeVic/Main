using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;

public class DashboardInitResponse : IResponse
{
    public UserDto CurrentUser { get; set; } = null!;
    
    public ICollection<WorkspaceDto> Workspaces { get; set; } = new List<WorkspaceDto>();
    
    public WorkspaceDto CurrentWorkspace { get; set; } = null!;
    
    public ICollection<WorkspacePermission> Permissions { get; set; } = new List<WorkspacePermission>();
    
    public ICollection<WorkspaceMemberDto> WorkspaceMembers { get; set; } = new List<WorkspaceMemberDto>();
    
    public ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    
    public ICollection<ClientDto> Clients { get; set; } = new List<ClientDto>();
    
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
    
    public ICollection<TaskListForListDto> TaskLists { get; set; } = new List<TaskListForListDto>();
    
    public TimeEntryDto? ActiveTimeEntry { get; set; }
}
