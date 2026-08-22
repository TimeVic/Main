using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals.Effects;

public class UnapproveEntriesEffect : Effect<UnapproveEntriesAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UnapproveEntriesEffect> _logger;

    public UnapproveEntriesEffect(
        IApiService apiService,
        ILogger<UnapproveEntriesEffect> logger
    )
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(UnapproveEntriesAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(true));
            foreach (var id in action.TimeEntryIds)
            {
                await _apiService.TimeEntryApprovalUnapproveAsync(id);
            }
            dispatcher.Dispatch(new FetchSubmittersAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error unapproving entries: {Message}", e.Message);
            dispatcher.Dispatch(new SetErrorMessageAction(e.Message));
        }
        finally
        {
            dispatcher.Dispatch(new SetIsActionProcessingAction(false));
        }
    }
}
