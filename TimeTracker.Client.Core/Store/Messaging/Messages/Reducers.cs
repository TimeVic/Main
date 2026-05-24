using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Client.Core.Store.Messaging.Messages;

public class Reducers
{
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, AddMessageAction action)
    {
        return state with
        {
            ListStates = state.ListStates.Select(item =>
            {
                if (item.Channel == action.Message.Channel)
                {
                    return item with
                    {
                        List = item.List.Concat([action.Message]).ToList(),
                        TotalCount = item.TotalCount + 1
                    };
                }

                return item;
            }).ToList()
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, SetPageAction action)
    {
        return state with
        {
            ListStates = state.ListStates.Select(item =>
            {
                if (item.Channel == action.Channel)
                {
                    return item with
                    {
                        Page = action.Page
                    };
                }
                return item;
            }).ToList()
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
            ListStates = state.ListStates.Select(item =>
            {
                if (item.Channel == action.Channel)
                {
                    return item with
                    {
                        Page = 1,
                        TotalCount = 0,
                        IsListFullListLoaded = false,
                        List = new List<MessagingMessageDto>()
                    };
                }
                return item;
            }).ToList()
        };
    }
    
    [ReducerMethod]
    public static MessagesState Reducer(MessagesState state, SetListAction action)
    {
        var listState = state.GetListState(action.Channel);
        if (listState == null)
        {
            listState = new MessagesListState()
            {
                Channel = action.Channel,
                TotalCount = 0,
                List = [],
            };
            state.ListStates.Add(listState);
        }
        listState.TotalCount = action.Response.TotalCount;
        listState.List = listState.List.Concat(action.Response.Items).ToList();
        listState.IsListFullListLoaded = !action.Response.IsHasMore;

        return state with
        {
            ListStates = state.ListStates.Select(item =>
            {
                if (item.Channel == action.Channel)
                {
                    return listState;
                }
                return item;
            }).ToList()
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
