using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals.Effects;

public class RejectEntriesEffect : Effect<RejectEntriesAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<RejectEntriesEffect> _logger;

    public RejectEntriesEffect(
        IApiService apiService,
        ILogger<RejectEntriesEffect> logger
    )
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(RejectEntriesAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(true));
            await _apiService.TimeEntryApprovalRejectAsync(action.TimeEntryIds, action.Reason);
            dispatcher.Dispatch(new FetchSubmittersAction());
            dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Dashboard.FetchCountersAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error rejecting entries: {Message}", e.Message);
            dispatcher.Dispatch(new SetErrorMessageAction(e.Message));
        }
        finally
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(false));
        }
    }
}
