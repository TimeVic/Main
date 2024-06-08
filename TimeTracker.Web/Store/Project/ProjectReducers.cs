using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Project;

public class ProjectReducers
{

    [ReducerMethod]
    public static ProjectState SetProjectListItemsActionReducer(ProjectState state, SetListItemsAction action)
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
    public static ProjectState SetProjectIsListLoadingReducer(ProjectState state, SetProjectIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static ProjectState SetListItemActionReducer(ProjectState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.Project.Id)
            {
                return action.Project;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.Project.Id))
        {
            list.Insert(0, action.Project);
        }

        return state with
        {
            List = list
        };
    }
}
