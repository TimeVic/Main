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
    public static TasksState SetListItemActionReducer(TasksState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.Task.Id)
            {
                return action.Task;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.Task.Id))
        {
            list.Insert(0, action.Task);
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
}
