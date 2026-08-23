using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals.Effects;

public class FetchSubmittersEffect : Effect<FetchSubmittersAction>
{
    private readonly IApiService _apiService;
    private readonly IState<ApprovalsState> _state;
    private readonly ILogger<FetchSubmittersEffect> _logger;

    public FetchSubmittersEffect(
        IApiService apiService,
        IState<ApprovalsState> state,
        ILogger<FetchSubmittersEffect> logger
    )
    {
        _apiService = apiService;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(FetchSubmittersAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsLoadingAction(true));
            var response = await _apiService.TimeEntryApprovalGetSubmittersAsync();
            if (response != null)
            {
                dispatcher.Dispatch(new SetSubmittersAction(response));

                // Auto fetch details for selected submitter if available
                var selected = _state.Value.SelectedSubmitter;
                if (selected != null)
                {
                    dispatcher.Dispatch(new FetchApprovalDetailsAction(
                        selected.UserId,
                        selected.PeriodStartDate,
                        selected.PeriodEndDate
                    ));
                }
                else
                {
                    dispatcher.Dispatch(new SetApprovalDetailsAction(null));
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching approval submitters: {Message}", e.Message);
            dispatcher.Dispatch(new SetErrorMessageAction(e.Message));
        }
        finally
        {
            dispatcher.Dispatch(new SetIsLoadingAction(false));
        }
    }
}
