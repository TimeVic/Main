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
    public static PaymentState SetPaymentListItemActionReducer(PaymentState state, SetListItemAction action)
    {
        foreach (var item in state.List)
        {
            if (item.Id == action.Payment.Id)
            {
                item.Amount = action.Payment.Amount;
                item.Client = action.Payment.Client;
                item.Description = action.Payment.Description;
                item.Project = action.Payment.Project;
                item.PaymentTime = action.Payment.PaymentTime;
            }
        }
        return state;
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
