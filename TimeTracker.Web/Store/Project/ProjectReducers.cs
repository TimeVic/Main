using Fluxor;

namespace TimeTracker.Web.Store.Project;

public class ProjectReducers
{

    [ReducerMethod]
    public static ProjectState SetProjectListItemsActionReducer(ProjectState state, SetProjectListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.TotalCount,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore
        };
    }
    
    [ReducerMethod]
    public static ProjectState SetProjectIsListLoadingReducer(ProjectState state, SetProjectIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
