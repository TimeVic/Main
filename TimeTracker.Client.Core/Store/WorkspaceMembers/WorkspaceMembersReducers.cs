using Fluxor;

namespace TimeTracker.Client.Core.Store.WorkspaceMembers;

public class ClientReducers
{

    [ReducerMethod]
    public static WorkspaceMembersState SetClientListItemsActionReducer(WorkspaceMembersState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.TotalCount,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static WorkspaceMembersState SetClientIsListLoadingReducer(WorkspaceMembersState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
