using Fluxor;

namespace TimeTracker.Client.Core.Store.Auth;

public class AuthReducers
{
    [ReducerMethod(typeof(LogoutAction))]
    public static AuthState Reducer(AuthState state)
    {
        return new AuthState();
    }
    
    [ReducerMethod]
    public static AuthState Reducer(AuthState state, LoginAction action)
    {
        return new AuthState()
        {
            Workspace = action.Workspace,
            User = action.User
        };
    }
    
    [ReducerMethod]
    public static AuthState Reducer(AuthState state, SetWorkspaceAction action)
    {
        return state with
        {
            Workspace = action.Workspace
        };
    }

    [ReducerMethod]
    public static AuthState Reducer(AuthState state, UpdateUserAction action)
    {
        return state with
        {
            User = action.User,
            Workspace = state.Workspace ?? action.User.SelectedWorkspace ?? action.User.DefaultWorkspace
        };
    }
}
