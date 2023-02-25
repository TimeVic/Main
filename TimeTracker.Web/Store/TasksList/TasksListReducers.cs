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
    public static TasksListState SetListItemActionReducer(TasksListState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.TaskList.Id)
            {
                return action.TaskList;
            }

            return item;
        }).ToList();
        return state with
        {
            List = list
        };
    }
    
    [ReducerMethod]
    public static TasksListState RemoveListItemsActionReducer(TasksListState state, RemoveListItemsAction action)
    {
        var list = state.List.Where(item => item.Id != action.TaskListId).ToList();
        return state with
        {
            List = list
        };
    }

    [ReducerMethod]
    public static TasksListState SetProjectIsListLoadingReducer(TasksListState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static TasksListState SetSelectedReducer(TasksListState state, SetSelectedAction action)
    {
        return state with
        {
            SelectedTaskListId = action.TaskListId
        };
    }
}
