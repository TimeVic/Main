using Fluxor;

namespace TimeTracker.Web.Store.Auth;

[FeatureState]
public class AuthState
{
    public bool IsLoggedIn => !string.IsNullOrEmpty(Jwt);

    public string Jwt { get; set; }

    public AuthState() { }
    
    public AuthState(string jwt)
    {
        Jwt = jwt;
    }
}
