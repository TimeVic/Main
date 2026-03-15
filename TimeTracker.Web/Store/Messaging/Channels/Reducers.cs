using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Channels;

public class Reducers
{
    [ReducerMethod]
    public static ChannelsState Reducer(ChannelsState state, RefreshListAction action)
    {
        return state with
        {
            List = new List<MessagingChannelDto>()
        };
    }
    
    [ReducerMethod]
    public static ChannelsState Reducer(ChannelsState state, SetListAction action)
    {
        return state with
        {
            List = state.List.Concat(action.Response.Items).ToList()
        };
    }
    
    [ReducerMethod]
    public static ChannelsState Reducer(ChannelsState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
