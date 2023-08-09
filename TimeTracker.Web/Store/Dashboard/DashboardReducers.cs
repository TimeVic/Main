using Fluxor;

namespace TimeTracker.Web.Store.Dashboard;

public class DashboardReducers
{
    [ReducerMethod]
    public static DashboardState SetProjectListItemsActionReducer(DashboardState state, SetTasksListItemsAction action)
    {
        return state with
        {
            MyTasks = new MyTasksState()
            {
                List = action.Response.Items,
                TotalCount = action.Response.TotalCount,
                TotalPages = action.Response.TotalPages,
                HasMoreItems = action.Response.IsHasMore,
            }
        };
    }

    [ReducerMethod]
    public static DashboardState SetProjectListItemActionReducer(DashboardState state, SetTasksListItemAction action)
    {
        return state with
        {
            MyTasks = state.MyTasks with
            {
                List = state.MyTasks.List.Select(item =>
                {
                    if (item.TaskId == action.Task.TaskId)
                    {
                        return action.Task;
                    }
                    return item;
                }).ToList()
            }
        };
    }

    [ReducerMethod]
    public static DashboardState SetIsTasksListLoadingActionReducer(DashboardState state,
        SetIsTasksListLoadingAction action)
    {
        return state with
        {
            MyTasks = state.MyTasks with
            {
                IsLoading = action.IsLoading
            }
        };
    }
}
