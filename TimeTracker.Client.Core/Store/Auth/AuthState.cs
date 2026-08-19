using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Client.Core.Store.Auth;

[FeatureState]
public record AuthState
{
    public bool IsLoggedIn => User != null && User.Id != Guid.Empty;
    
    public UserDto User { get; set; }
    
    public WorkspaceDto? Workspace { get; set; }

    public MembershipAccessType AccessLevel
    {
        get => Workspace?.CurrentUserAccess ?? MembershipAccessType.User;
    }
    
    public bool IsRoleUser => AccessLevel == MembershipAccessType.User;

    public bool IsRoleAdmin => AccessLevel is MembershipAccessType.Owner or MembershipAccessType.Manager;

    public bool IsRoleOwner => AccessLevel == MembershipAccessType.Owner;

    public AuthState() { }
}
