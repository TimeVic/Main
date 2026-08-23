using Fluxor;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals;

public class ApprovalsReducers
{
    [ReducerMethod]
    public static ApprovalsState SetSubmittersReducer(ApprovalsState state, SetSubmittersAction action)
    {
        TimeEntryApprovalSubmitterDto? selected = null;
        if (state.SelectedSubmitter != null)
        {
            selected = action.Response.Items.FirstOrDefault(i =>
                i.UserId == state.SelectedSubmitter.UserId && i.PeriodStartDate == state.SelectedSubmitter.PeriodStartDate
            );
        }

        var isSameSelected = selected != null && state.SelectedSubmitter != null
            && selected.UserId == state.SelectedSubmitter.UserId
            && selected.PeriodStartDate == state.SelectedSubmitter.PeriodStartDate;

        return state with
        {
            Submitters = action.Response.Items,
            SelectedSubmitter = selected,
            Details = isSameSelected ? state.Details : null,
            IsLoading = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static ApprovalsState SelectSubmitterReducer(ApprovalsState state, SelectSubmitterAction action)
    {
        var isSameSelected = action.Submitter != null && state.SelectedSubmitter != null
            && action.Submitter.UserId == state.SelectedSubmitter.UserId
            && action.Submitter.PeriodStartDate == state.SelectedSubmitter.PeriodStartDate;

        return state with
        {
            SelectedSubmitter = action.Submitter,
            Details = isSameSelected ? state.Details : null
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
