using Fluxor;

namespace TimeTracker.Client.Core.Store.Dashboard;

public class DashboardReducers
{
    [ReducerMethod]
    public static DashboardState SetCountersReducer(DashboardState state, SetCountersAction action)
    {
        return state with
        {
            Counters = action.Counters
        };
    }
}
