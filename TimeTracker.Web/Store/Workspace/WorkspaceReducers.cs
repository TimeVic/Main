using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Workspace;

public class ClientReducers
{

    [ReducerMethod]
    public static WorkspaceState Reducer(WorkspaceState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.Items.Count,
            IsLoaded = true
        };
    }
    
    [ReducerMethod]
    public static WorkspaceState Reducer(WorkspaceState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static WorkspaceState Reducer(WorkspaceState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.Workspace.Id)
            {
                return action.Workspace;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.Workspace.Id))
        {
            list.Insert(0, action.Workspace);
        }

        return state with
        {
            List = list
        };
    }
}
