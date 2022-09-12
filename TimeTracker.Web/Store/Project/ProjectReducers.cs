using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

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
    
    #region Add new item
    
    [ReducerMethod(typeof(AddEmptyProjectListItemAction))]
    public static ProjectState AddEmptyProjectListItemActionAction(ProjectState state)
    {
        var newList = state.SortedList.ToList();
        newList.Add(new ProjectDto()
        {
            Id = 0,
            Name = ""
        });
        return state with
        {
            List = newList,
            TotalCount = ++state.TotalCount
        };
    }
    
    [ReducerMethod(typeof(RemoveEmptyProjectListItemAction))]
    public static ProjectState RemoveEmptyProjectListItemActionAction(ProjectState state)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != 0).ToList(),
            TotalCount = --state.TotalCount
        };
    }
    
    #endregion
}
