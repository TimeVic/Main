using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Messages;

public class Reducers
{
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, AddMessageAction action)
    {
        var channel = state.List.Select(message => message.Channel).FirstOrDefault();
        if (channel == null || channel.Id == action.Message.Channel.Id)
        {
            state.List.Add(action.Message);       
        }
        return state with
        {
            List = state.List
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, SetIsMessageSending action)
    {
        return state with
        {
            IsMessageSending = action.IsSending
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, RefreshListAction action)
    {
        return state with
        {
            TotalCount = 0,
            List = new List<MessagingMessageDto>()
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, SetListAction action)
    {
        return state with
        {
            TotalCount = action.Response.TotalCount,
            List = state.List.Concat(action.Response.Items).ToList()
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
