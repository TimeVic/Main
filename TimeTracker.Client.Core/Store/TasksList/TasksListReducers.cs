using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Client.Core.Store.TasksList;

public class TasksListReducers
{
    [ReducerMethod]
    public static TasksListState Reducer(TasksListState state, SetListItemsAction action)
    {
        var list = action.Response.Items.Cast<TaskListDto>().ToList();
        var selectedTaskListId = list.Any(item => item.Id == state.SelectedTaskListId)
            ? state.SelectedTaskListId
            : null;

        return state with
        {
            List = list,
            TotalCount = list.Count,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore,
            IsLoaded = true,
            SelectedProjectId = action.ProjectId,
            SelectedTaskListId = selectedTaskListId
        };
    }

    [ReducerMethod]
    public static TasksListState Reducer(TasksListState state, SetDropDownListItemsAction action)
    {
        return state with
        {
            DropDownList = action.Response.Items.Cast<TaskListDto>().ToList(),
            DropDownProjectId = action.ProjectId
        };
    }
    
    [ReducerMethod]
    public static TasksListState Reducer(TasksListState state, SetListItemAction action)
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
    public static TasksListState Reducer(TasksListState state, RemoveListItemsAction action)
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
    public static TasksListState Reducer(TasksListState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static TasksListState Reducer(TasksListState state, SetSelectedAction action)
    {
        return state with
        {
            SelectedTaskListId = action.TaskListId
        };
    }
}
