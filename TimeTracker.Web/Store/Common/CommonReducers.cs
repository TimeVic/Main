using Fluxor;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Store.Common;

public class CommonReducers
{
    [ReducerMethod]
    public static CommonState SetIsAppInitializedActionReducer(CommonState state, SetIsAppInitializedAction action)
    {
        return state with
        {
            IsInitialized = action.IsInitialized
        };
    }
    
    [ReducerMethod]
    public static CommonState SetIsWorkspaceInitializedActionReducer(CommonState state, SetIsWorkspaceInitializedAction action)
    {
        return state with
        {
            IsWorkspaceInitialized = action.IsInitialized
        };
    }
}
