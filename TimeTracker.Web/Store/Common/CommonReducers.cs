using Fluxor;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Common.Actions;

namespace TimeTracker.Web.Store.Common;

public class CommonReducers
{
    [ReducerMethod]
    public static CommonState ReduceLogoutActionAction(CommonState state, SetIsAppInitializedAction action)
    {
        return new CommonState(action.IsInitialized);
    }
}
