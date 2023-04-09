using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Web.Store.Auth;

[FeatureState]
public record AuthState
{
    public bool IsLoggedIn => !string.IsNullOrEmpty(Jwt);

    public string Jwt { get; set; }
    
    public UserDto User { get; set; }
    
    public WorkspaceDto? Workspace { get; set; }

    public MembershipAccessType AccessLevel
    {
        get => Workspace?.CurrentUserAccess ?? MembershipAccessType.User;
    }
    
    public bool IsRoleUser => AccessLevel == MembershipAccessType.User;

    public bool IsRoleAdmin => AccessLevel is MembershipAccessType.Owner or MembershipAccessType.Manager;

    public AuthState() { }
}
