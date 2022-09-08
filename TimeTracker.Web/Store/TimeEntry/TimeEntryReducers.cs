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
            List = action.List.Items,
            TotalCount = action.List.TotalCount,
            TotalPages = action.List.TotalPages,
            HasMoreItems = action.List.IsHasMore,
            ActiveEntry = action.List.Items.FirstOrDefault(item => item.IsActive)
        };
    }
}
