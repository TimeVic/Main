using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Payment;

public class ClientReducers
{

    [ReducerMethod]
    public static PaymentState SetPaymentListItemsActionReducer(PaymentState state, SetListItemsAction action)
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
    public static PaymentState SetPaymentIsListLoadingReducer(PaymentState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static PaymentState SetListItemActionReducer(PaymentState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.Payment.Id)
            {
                return action.Payment;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.Payment.Id))
        {
            list.Insert(0, action.Payment);
        }

        return state with
        {
            List = list
        };
    }
    
    [ReducerMethod]
    public static PaymentState RemovePaymentListItemActionReducer(PaymentState state, RemovePaymentListItemAction action)
    {
        return state with
        {
            TotalCount = --state.TotalCount,
            List = state.List.Where(item => item.Id != action.PaymentId).ToList()
        };
    }
}
