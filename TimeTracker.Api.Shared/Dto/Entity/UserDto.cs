using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class UserDto: BaseDto
{   
    public string? UserName { get; set; }
    
    public string Email { get; set; }
    
    public string Timezone { get; set; }

    public string Name
    {
        get => string.IsNullOrEmpty(UserName) ? Email : UserName;
    }
    
    public string Initials
    {
        get => Name.GetFirstUpperLetters(2);
    }
    
    public WorkspaceDto? DefaultWorkspace { get; set; }
}
