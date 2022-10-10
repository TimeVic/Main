using Fluxor;

namespace TimeTracker.Web.Store.WorkspaceMemberships;

public class ClientReducers
{

    [ReducerMethod]
    public static WorkspaceMembershipsState SetClientListItemsActionReducer(WorkspaceMembershipsState state, SetListItemsAction action)
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
    public static WorkspaceMembershipsState SetClientIsListLoadingReducer(WorkspaceMembershipsState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
