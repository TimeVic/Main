using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class UserDto: BaseDto
{   
    public string? UserName { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string? Login { get; set; }
    
    public string Timezone { get; set; } = string.Empty;

    public string Name
    {
        get => !string.IsNullOrEmpty(UserName) ? UserName : (!string.IsNullOrEmpty(Login) ? $"@{Login}" : Email);
    }

    public string FormattedLogin
    {
        get => !string.IsNullOrEmpty(Login) ? $"@{Login}" : Email;
    }
    
    public string Initials
    {
        get => Name.TrimStart('@').GetFirstUpperLetters(2);
    }
    
    public WorkspaceDto? DefaultWorkspace { get; set; }

    public WorkspaceDto? SelectedWorkspace { get; set; }

    public LanguageDto? Language { get; set; }
    
    public StoredFileDto? Avatar { get; set; }
}
