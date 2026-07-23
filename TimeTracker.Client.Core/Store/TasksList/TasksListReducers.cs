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
        var list = UpsertTaskList(state.List, action.TaskList);
        var dropDownList = state.DropDownList
            .Where(item => item.Id != action.TaskList.Id)
            .ToList();
        if (state.DropDownProjectId == action.TaskList.Project.Id)
        {
            dropDownList.Insert(0, action.TaskList);
        }

        return state with
        {
            List = list,
            DropDownList = dropDownList,
            TotalCount = list.Count
        };
    }
    
    [ReducerMethod]
    public static TasksListState Reducer(TasksListState state, RemoveListItemsAction action)
    {
        var list = state.List.Where(item => item.Id != action.TaskListId).ToList();
        return state with
        {
            List = list,
            DropDownList = state.DropDownList
                .Where(item => item.Id != action.TaskListId)
                .ToList(),
            TotalCount = list.Count,
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

    private static List<TaskListDto> UpsertTaskList(
        IEnumerable<TaskListDto> taskLists,
        TaskListDto taskList
    )
    {
        var list = taskLists
            .Where(item => item.Id != taskList.Id)
            .ToList();
        list.Insert(0, taskList);
        return list;
    }
}
