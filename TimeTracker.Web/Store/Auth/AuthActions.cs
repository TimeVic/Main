namespace TimeTracker.Web.Store.Auth;

public record struct LogoutAction();

public record struct LoginAction(string Jwt)
{
    public LoginAction(AuthState state) : this(state.Jwt)
    {
    }
}
