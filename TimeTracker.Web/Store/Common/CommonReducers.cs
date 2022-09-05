using Fluxor;
using TimeTracker.Web.Store.Common.Actions;

namespace TimeTracker.Web.Store.Common;

public class CommonReducers
{
    [ReducerMethod(typeof(SetIsAppInitializedAction))]
    public static CommonState ReduceLogoutActionAction(CommonState state)
    {
        return new CommonState(true);
    }
}
