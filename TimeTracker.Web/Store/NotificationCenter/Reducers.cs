using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.NotificationCenter;

namespace TimeTracker.Web.Store.NotificationCenter;

public class Reducers
{

    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, SetUnreadCountAction action)
    {
        return state with
        {
            UnreadCount = action.Count
        };
    }
    
    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, RefreshListAction action)
    {
        return state with
        {
            NextPage = 1,
            List = new List<NotificationDto>(),
            IsListHasMore = false
        };
    }
    
    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, SetListAction action)
    {
        return state with
        {
            IsListHasMore = action.Response.IsHasMore,
            NextPage = state.NextPage + 1,
            List = state.List.Concat(action.Response.Items).ToList()
        };
    }
    
    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, SetIsListLoadingAction action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, SetAllAsReadAction action)
    {
        return state with
        {
            List = state.List.Select(item =>
            {
                item.IsRead = true;
                return item;
            }).ToList()
        };
    }
    
    [ReducerMethod]
    public static NotificationCenterState Reducer(NotificationCenterState state, SetAsReadAction action)
    {
        return state with
        {
            List = state.List.Select(item =>
            {
                if (item.Id == action.NotificationId)
                {
                    item.IsRead = true;    
                }
                return item;
            }).ToList()
        };
    }
}
