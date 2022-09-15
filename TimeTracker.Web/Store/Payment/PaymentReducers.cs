using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Payment;

public class ClientReducers
{

    [ReducerMethod]
    public static PaymentState SetPaymentListItemsActionReducer(PaymentState state, SetPaymentListItemsAction action)
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
    public static PaymentState SetPaymentIsListLoadingReducer(PaymentState state, SetPaymentIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    #region Add new item
    
    [ReducerMethod(typeof(AddEmptyPaymentListItemAction))]
    public static PaymentState AddEmptyPaymentListItemActionAction(PaymentState state)
    {
        var newList = state.SortedList.ToList();
        newList.Add(new PaymentDto()
        {
            Id = 0,
            Description = ""
        });
        return state with
        {
            List = newList,
            TotalCount = ++state.TotalCount
        };
    }
    
    [ReducerMethod(typeof(RemoveEmptyPaymentListItemAction))]
    public static PaymentState RemoveEmptyPaymentListItemActionAction(PaymentState state)
    {
        return state with
        {
            List = state.List.Where(item => item.Id != 0).ToList(),
            TotalCount = --state.TotalCount
        };
    }
    
    #endregion
}
