using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Workspace;

public class ClientReducers
{

    [ReducerMethod]
    public static WorkspaceState SetListItemsActionReducer(WorkspaceState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.Items.Count,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static WorkspaceState SetIsListLoadingReducer(WorkspaceState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    #region Add new item
    
    [ReducerMethod(typeof(AddEmptyListItemAction))]
    public static WorkspaceState AddEmptyListItemActionAction(WorkspaceState state)
    {
        var newList = state.SortedList.ToList();
        newList.Add(new WorkspaceDto()
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
    
    [ReducerMethod(typeof(RemoveEmptyListItemAction))]
    public static WorkspaceState RemoveEmptyWorkspaceListItemActionAction(WorkspaceState state)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != 0).ToList(),
            TotalCount = --state.TotalCount
        };
    }
    
    #endregion
}
