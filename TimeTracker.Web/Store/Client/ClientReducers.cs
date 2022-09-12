using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Client;

public class ClientReducers
{

    [ReducerMethod]
    public static ClientState SetClientListItemsActionReducer(ClientState state, SetClientListItemsAction action)
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
    public static ClientState SetClientIsListLoadingReducer(ClientState state, SetClientIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    #region Add new item
    
    [ReducerMethod(typeof(AddEmptyClientListItemAction))]
    public static ClientState AddEmptyClientListItemActionAction(ClientState state)
    {
        var newList = state.SortedList.ToList();
        newList.Add(new ClientDto()
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
    
    [ReducerMethod(typeof(RemoveEmptyClientListItemAction))]
    public static ClientState RemoveEmptyClientListItemActionAction(ClientState state)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != 0).ToList(),
            TotalCount = --state.TotalCount
        };
    }
    
    #endregion
}
