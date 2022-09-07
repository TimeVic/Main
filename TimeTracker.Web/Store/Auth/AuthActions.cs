using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Auth;

public record struct LogoutAction();

public record struct LoginAction(string Jwt, UserDto User)
{
    public LoginAction(AuthState state) : this(state.Jwt, state.User)
    {
    }
}
