using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.TasksList;

public class TasksListReducers
{

    [ReducerMethod]
    public static TasksListState SetProjectListItemsActionReducer(TasksListState state, SetListItemsAction action)
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
    public static TasksListState SetProjectIsListLoadingReducer(TasksListState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static TasksListState SetSelectedReducer(TasksListState state, SetSelected action)
    {
        return state with
        {
            SelectedTaskListId = action.TaskListId
        };
    }
}
