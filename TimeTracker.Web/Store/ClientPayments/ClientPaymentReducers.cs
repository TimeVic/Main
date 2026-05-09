using Fluxor;

namespace TimeTracker.Web.Store.ClientPayments;

public class ClientPaymentReducers
{
    [ReducerMethod]
    public static ClientPaymentState SetClientPaymentListItemsActionReducer(
        ClientPaymentState state,
        SetClientPaymentListItemsAction action
    )
    {
        return state with
        {
            List = action.Response.Items,
            TotalCount = action.Response.TotalCount,
            TotalPages = action.Response.TotalPages,
            HasMoreItems = action.Response.IsHasMore,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static ClientPaymentState SetClientPaymentSelectedPageActionReducer(
        ClientPaymentState state,
        SetClientPaymentSelectedPageAction action
    )
    {
        return state with
        {
            SelectedPage = action.SelectedPage
        };
    }

    [ReducerMethod]
    public static ClientPaymentState SetClientPaymentIsListLoadingReducer(
        ClientPaymentState state,
        SetClientPaymentIsListLoadingAction action
    )
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }

    [ReducerMethod]
    public static ClientPaymentState SetClientPaymentListItemActionReducer(
        ClientPaymentState state,
        SetClientPaymentListItemAction action
    )
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.ClientPayment.Id)
            {
                return action.ClientPayment;
            }

            return item;
        }).ToList();

        if (list.All(item => item.Id != action.ClientPayment.Id))
        {
            list.Insert(0, action.ClientPayment);
        }

        return state with
        {
            List = list
        };
    }

    [ReducerMethod]
    public static ClientPaymentState RemoveClientPaymentListItemActionReducer(
        ClientPaymentState state,
        RemoveClientPaymentListItemAction action
    )
    {
        return state with
        {
            TotalCount = Math.Max(0, state.TotalCount - 1),
            List = state.List.Where(item => item.Id != action.ClientPaymentId).ToList()
        };
    }
}
