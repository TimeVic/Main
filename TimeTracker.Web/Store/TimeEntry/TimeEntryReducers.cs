using Fluxor;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Store.TimeEntry;

public class TimeEntryReducers
{
    [ReducerMethod]
    public static TimeEntryState SetActiveTimeEntryActionReducer(TimeEntryState state, SetActiveTimeEntryAction action)
    {
        return state with
        {
            ActiveEntry = action.TimeEntry
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetActiveTimeEntryActionReducer(TimeEntryState state, SetTimeEntryListItemsAction action)
    {
        return state with
        {
            List = action.Response.List.Items,
            TotalCount = action.Response.List.TotalCount,
            TotalPages = action.Response.List.TotalPages,
            HasMoreItems = action.Response.List.IsHasMore,
            ActiveEntry = action.Response.ActiveTimeEntry
        };
    }
    
    [ReducerMethod]
    public static TimeEntryState SetTimeEntryIsListLoadingReducer(TimeEntryState state, SetTimeEntryIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
}
