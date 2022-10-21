namespace TimeTracker.Api.Shared.Dto.Entity;

public class UserDto
{
    public long Id { get; set; }
    
    public string? UserName { get; set; }
    
    public string Email { get; set; }
    
    public string Timezone { get; set; }

    public string Name
    {
        get => string.IsNullOrEmpty(UserName) ? Email : UserName;
    }
    
    public WorkspaceDto? DefaultWorkspace { get; set; }
}
