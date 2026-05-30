using Fluxor;

namespace TimeTracker.Client.Core.Store.List.Currency;

public class CurrencyReducers
{

    [ReducerMethod]
    public static CurrencyState Reducer(CurrencyState state, SetListItemsAction action)
    {
        return state with
        {
            List = action.Items
        };
    }

    [ReducerMethod]
    public static CurrencyState Reducer(CurrencyState state, SetIsLoading action)
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }
}
