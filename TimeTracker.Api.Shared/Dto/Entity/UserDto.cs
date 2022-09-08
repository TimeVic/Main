namespace TimeTracker.Api.Shared.Dto.Entity;

public class UserDto
{
    public long Id { get; set; }
    
    public string? UserName { get; set; }
    
    public string Email { get; set; }
    
    public WorkspaceDto DefaultWorkspace { get; set; }
}
