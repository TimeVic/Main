using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Channels;

public class Reducers
{
    [ReducerMethod]
    public static ChannelsState Reducer(ChannelsState state, AddChannelAction action)
    {
        state.List.Add(action.Channel);
        return state with
        {
            List = state.List
        };
    }
    
    [ReducerMethod]
    public static ChannelsState Reducer(ChannelsState state, SetSelectedAction action)
    {
        return state with
        {
            SelectedId = action.Channel?.Id
        };
    }
    
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
