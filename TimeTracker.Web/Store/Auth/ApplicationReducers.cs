using Fluxor;

namespace TimeTracker.Web.Store.Auth;

public class AuthReducers
{
    [ReducerMethod(typeof(LogoutAction))]
    public static AuthState ReduceLogoutActionActionReducer(AuthState state)
    {
        return new AuthState();
    }
    
    [ReducerMethod]
    public static AuthState ReduceLoginActionActionReducer(AuthState state, LoginAction action)
    {
        return new AuthState()
        {
            Jwt = action.Jwt,
            Workspace = action.Workspace,
            User = action.User
        };
    }
    
    [ReducerMethod]
    public static AuthState SetWorkspaceActionReducer(AuthState state, SetWorkspaceAction action)
    {
        return state with
        {
            Workspace = action.Workspace
        };
    }
}
