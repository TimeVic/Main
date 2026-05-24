using Fluxor;

namespace TimeTracker.Client.Core.Store.Common;

public class CommonReducers
{
    [ReducerMethod]
    public static CommonState Reducer(CommonState state, SetIsAppInitializedAction action)
    {
        return state with
        {
            IsInitialized = action.IsInitialized
        };
    }
    
    [ReducerMethod]
    public static CommonState Reducer(CommonState state, SetIsWorkspaceInitializedAction action)
    {
        return state with
        {
            IsWorkspaceInitialized = action.IsInitialized
        };
    }
}
