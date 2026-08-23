using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals.Effects;

public class ApproveEntriesEffect : Effect<ApproveEntriesAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<ApproveEntriesEffect> _logger;

    public ApproveEntriesEffect(
        IApiService apiService,
        ILogger<ApproveEntriesEffect> logger
    )
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(ApproveEntriesAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(true));
            await _apiService.TimeEntryApprovalApproveAsync(action.TimeEntryIds);
            dispatcher.Dispatch(new FetchSubmittersAction());
            dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Dashboard.FetchCountersAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error approving entries: {Message}", e.Message);
            dispatcher.Dispatch(new SetErrorMessageAction(e.Message));
        }
        finally
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(false));
        }
    }
}
