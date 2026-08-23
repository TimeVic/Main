using Fluxor;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals;

public class ApprovalsReducers
{
    [ReducerMethod]
    public static ApprovalsState SetSubmittersReducer(ApprovalsState state, SetSubmittersAction action)
    {
        var selected = state.SelectedSubmitter;
        if (selected != null)
        {
            selected = action.Response.Items.FirstOrDefault(i =>
                i.UserId == selected.UserId && i.PeriodStartDate == selected.PeriodStartDate
            ) ?? action.Response.Items.FirstOrDefault();
        }
        else
        {
            selected = action.Response.Items.FirstOrDefault();
        }

        return state with
        {
            Submitters = action.Response.Items,
            SelectedSubmitter = selected,
            IsLoading = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static ApprovalsState SelectSubmitterReducer(ApprovalsState state, SelectSubmitterAction action)
    {
        return state with
        {
            SelectedSubmitter = action.Submitter
        };
    }

    [ReducerMethod]
    public static ApprovalsState SetApprovalDetailsReducer(ApprovalsState state, SetApprovalDetailsAction action)
    {
        return state with
        {
            Details = action.Details,
            IsDetailsLoading = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static ApprovalsState SetIsLoadingReducer(ApprovalsState state, SetIsLoadingAction action)
    {
        return state with
        {
            IsLoading = action.IsLoading
        };
    }

    [ReducerMethod]
    public static ApprovalsState SetIsDetailsLoadingReducer(ApprovalsState state, SetIsDetailsLoadingAction action)
    {
        return state with
        {
            IsDetailsLoading = action.IsLoading
        };
    }

    [ReducerMethod]
    public static ApprovalsState SetIsActionProcessingReducer(ApprovalsState state, SetIsActionProcessingAction action)
    {
        return state with
        {
            IsActionProcessing = action.IsProcessing
        };
    }

    [ReducerMethod]
    public static ApprovalsState SetErrorMessageReducer(ApprovalsState state, SetErrorMessageAction action)
    {
        return state with
        {
            ErrorMessage = action.ErrorMessage,
            IsLoading = false,
            IsDetailsLoading = false,
            IsActionProcessing = false
        };
    }
}
