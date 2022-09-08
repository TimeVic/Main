using Fluxor;

namespace TimeTracker.Web.Store.Auth;

public class AuthReducers
{
    [ReducerMethod(typeof(LogoutAction))]
    public static AuthState ReduceLogoutActionAction(AuthState state)
    {
        return new AuthState();
    }
    
    [ReducerMethod]
    public static AuthState ReduceLoginActionAction(AuthState state, LoginAction action)
    {
        return new AuthState()
        {
            Jwt = action.Jwt,
            Workspace = action.Workspace,
            User = action.User
        };
    }
}
