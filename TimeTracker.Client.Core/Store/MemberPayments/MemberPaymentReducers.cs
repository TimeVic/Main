using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Store.MemberPayments;

public class MemberPaymentReducers
{

    [ReducerMethod]
    public static MemberPaymentState SetMemberPaymentListItemsActionReducer(MemberPaymentState state, SetListItemsAction action)
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
    public static MemberPaymentState SetMemberPaymentSelectedPageActionReducer(
        MemberPaymentState state,
        SetMemberPaymentSelectedPageAction action
    )
    {
        return state with
        {
            SelectedPage = action.SelectedPage
        };
    }

    [ReducerMethod]
    public static MemberPaymentState SetMemberPaymentIsListLoadingReducer(MemberPaymentState state, SetIsListLoading action)
    {
        return state with
        {
            IsListLoading = action.IsLoading
        };
    }
    
    [ReducerMethod]
    public static MemberPaymentState SetListItemActionReducer(MemberPaymentState state, SetListItemAction action)
    {
        var list = state.List.Select(item =>
        {
            if (item.Id == action.MemberPayment.Id)
            {
                return action.MemberPayment;
            }
            return item;
        }).ToList();
        if (list.All(item => item.Id != action.MemberPayment.Id))
        {
            list.Insert(0, action.MemberPayment);
        }

        return state with
        {
            List = list
        };
    }
    
    [ReducerMethod]
    public static MemberPaymentState RemoveMemberPaymentListItemActionReducer(MemberPaymentState state, RemoveMemberPaymentListItemAction action)
    {
        return state with
        {
            TotalCount = Math.Max(0, state.TotalCount - 1),
            List = state.List.Where(item => item.Id != action.MemberPaymentId).ToList()
        };
    }
}
