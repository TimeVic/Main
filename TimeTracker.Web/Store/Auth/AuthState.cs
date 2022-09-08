using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Auth;

[FeatureState]
public class AuthState
{
    public bool IsLoggedIn => !string.IsNullOrEmpty(Jwt);

    public string Jwt { get; set; }
    
    public UserDto User { get; set; }
    
    public WorkspaceDto Workspace { get; set; }

    public AuthState() { }
}
