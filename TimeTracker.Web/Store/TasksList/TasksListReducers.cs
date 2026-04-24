using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Store.TasksList;

public class TasksListReducers
{
    [ReducerMethod]
    public static TasksListState SetProjectListItemsActionReducer(TasksListState state, SetListItemsAction action)
    {
        var list = action.Response.Items.ToList();
        var selectedTaskListId = list.Any(item => item.Id == state.SelectedTaskListId)
            ? state.SelectedTaskListId
            : null;

        return state with
        {
            List = list,
            TotalCount = list.Count,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore,
            IsLoaded = action.ProjectId.HasValue,
            SelectedProjectId = action.ProjectId,
            SelectedTaskListId = selectedTaskListId
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
        if (list.All(item => item.Id != action.TaskList.Id))
        {
            list.Insert(0, action.TaskList);
        }

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
            List = list,
            SelectedTaskListId = state.SelectedTaskListId == action.TaskListId
                ? null
                : state.SelectedTaskListId
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
