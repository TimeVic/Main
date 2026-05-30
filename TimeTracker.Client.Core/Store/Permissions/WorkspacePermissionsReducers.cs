using Fluxor;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Permissions;

public class WorkspacePermissionsReducers
{
    [ReducerMethod]
    public static WorkspacePermissionsState Reducer(
        WorkspacePermissionsState state,
        SetWorkspacePermissionsAction action
    )
    {
        return state with
        {
            WorkspaceId = action.Response.WorkspaceId,
            Permissions = action.Response.Permissions,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static WorkspacePermissionsState Reducer(
        WorkspacePermissionsState state,
        SetWorkspacePermissionsLoadingAction action
    )
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }

    [ReducerMethod(typeof(ClearWorkspacePermissionsAction))]
    public static WorkspacePermissionsState Reducer(WorkspacePermissionsState state)
    {
        return new WorkspacePermissionsState();
    }

    [ReducerMethod(typeof(LogoutAction))]
    public static WorkspacePermissionsState ReducerOnLogout(WorkspacePermissionsState state)
    {
        return new WorkspacePermissionsState();
    }

    [ReducerMethod(typeof(SetWorkspaceAction))]
    public static WorkspacePermissionsState ReducerOnWorkspaceChanged(WorkspacePermissionsState state)
    {
        return new WorkspacePermissionsState();
    }
}
