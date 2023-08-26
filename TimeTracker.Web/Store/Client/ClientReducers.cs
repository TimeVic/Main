using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Client;

public class ClientReducers
{

    [ReducerMethod]
    public static ClientState SetClientListItemsActionReducer(ClientState state, SetListItemsAction action)
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
    public static ClientState SetClientIsListLoadingReducer(ClientState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static ClientState SetListItemActionReducer(ClientState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.Client.Id)
            {
                return action.Client;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.Client.Id))
        {
            list.Insert(0, action.Client);
        }

        return state with
        {
            List = list
        };
    }
}
