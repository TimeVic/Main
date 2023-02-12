using Fluxor;

namespace TimeTracker.Web.Store.Tasks;

public class TasksReducers
{

    [ReducerMethod]
    public static TasksState SetProjectListItemsActionReducer(TasksState state, SetListItemsAction action)
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
    public static TasksState SetProjectIsListLoadingReducer(TasksState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
