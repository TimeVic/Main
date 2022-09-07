using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Auth;

[FeatureState]
public class AuthState
{
    public bool IsLoggedIn => !string.IsNullOrEmpty(Jwt);

    public string Jwt { get; set; }
    
    public UserDto User { get; set; }

    public long CurrentWorkspaceId { get; set; } = 1;

    public AuthState() { }
    
    public AuthState(string jwt, UserDto user)
    {
        Jwt = jwt;
        User = user;
    }
}
