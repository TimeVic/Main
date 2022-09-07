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
}
