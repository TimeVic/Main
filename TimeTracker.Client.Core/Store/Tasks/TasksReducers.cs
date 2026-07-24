using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Client.Core.Store.Tasks;

public class TasksReducers
{

    [ReducerMethod]
    public static TasksState SetListItemsActionReducer(TasksState state, SetListItemsAction action)
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
    public static TasksState SetListItemActionReducer(TasksState state, SetListItemAction action)
    {
        var list = GetMutableList(state.List);
        var taskIndex = list.FindIndex(item => item.TaskId == action.Task.TaskId);

        if (taskIndex >= 0)
        {
            list[taskIndex] = action.Task;
        }
        else
        {
            list.Insert(0, action.Task);
        }

        return state with
        {
            List = list
        };
    }

    [ReducerMethod]
    public static TasksState RemoveListItemActionReducer(TasksState state, RemoveListItemAction action)
    {
        var list = GetMutableList(state.List);
        var taskIndex = list.FindIndex(item => item.Id == action.TaskId);
        if (taskIndex >= 0)
        {
            list.RemoveAt(taskIndex);
        }

        return state with
        {
            List = list
        };
    }
    
    [ReducerMethod]
    public static TasksState UpdateListItemsActionReducer(TasksState state, UpdateListItemsAction action)
    {
        var updatedTasks = action.Tasks.ToDictionary(task => task.TaskId);
        var list = GetMutableList(state.List);

        for (var index = 0; index < list.Count; index++)
        {
            if (updatedTasks.TryGetValue(list[index].TaskId, out var updatedTask))
            {
                list[index] = updatedTask;
            }
        }

        return state with
        {
            List = list
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
    
    [ReducerMethod]
    public static TasksState SetListFilterActionReducer(TasksState state, SetListFilterAction action)
    {
        return state with
        {
            Filter = action.Filter
        };
    }
    
    [ReducerMethod]
    public static TasksState SetOverdueTasksListItemsActionReducer(TasksState state, SetOverdueTasksListItemsAction action)
    {
        return state with
        {
            OverdueList = action.Response.Items
        };
    }

    [ReducerMethod]
    public static TasksState SetOverdueTasksListItemActionReducer(TasksState state, SetOverdueTasksListItemAction action)
    {
        state = state with
        {
            OverdueList = state.OverdueList.Select(item =>
            {
                if (item.TaskId == action.Task.TaskId)
                {
                    return action.Task;
                }
                return item;
            }).ToList()
        };
        if (state.OverdueList.All(item => item.TaskId != action.Task.TaskId))
        {
            state.OverdueList.Add(action.Task);
        }

        return state;
    }

    [ReducerMethod]
    public static TasksState SetIsOverdueTasksListLoadingActionReducer(TasksState state, SetIsOverdueTasksListLoadingAction action)
    {
        return state with
        {
            IsOverdueListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static TasksState SetIsTaskSavingLoadingActionReducer(TasksState state, SetIsTaskSavingAction action)
    {
        return state with
        {
            IsTaskSaving = action.IsSaving
        };
    }

    [ReducerMethod]
    public static TasksState ToggleStatusExpansionReducer(TasksState state, ToggleStatusExpansionAction action)
    {
        var expandedStatuses = state.ExpandedStatuses.ToHashSet();
        if (!expandedStatuses.Add(action.Status))
        {
            expandedStatuses.Remove(action.Status);
        }

        return state with
        {
            ExpandedStatuses = expandedStatuses
        };
    }

    private static List<TaskDto> GetMutableList(ICollection<TaskDto> tasks) =>
        tasks as List<TaskDto> ?? tasks.ToList();
}
