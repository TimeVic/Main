using Fluxor;

namespace TimeTracker.Web.Store.Workspace;

public class ClientReducers
{

    [ReducerMethod]
    public static WorkspaceState SetListItemsActionReducer(WorkspaceState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static WorkspaceState SetIsListLoadingReducer(WorkspaceState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
