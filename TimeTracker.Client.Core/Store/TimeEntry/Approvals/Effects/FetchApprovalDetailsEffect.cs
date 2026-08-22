using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.TimeEntry.Approvals.Effects;

public class FetchApprovalDetailsEffect : Effect<FetchApprovalDetailsAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<FetchApprovalDetailsEffect> _logger;

    public FetchApprovalDetailsEffect(
        IApiService apiService,
        ILogger<FetchApprovalDetailsEffect> logger
    )
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(FetchApprovalDetailsAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsDetailsLoadingAction(true));
            var response = await _apiService.TimeEntryApprovalGetDetailsAsync(
                action.UserId,
                action.StartDate,
                action.EndDate
            );
            dispatcher.Dispatch(new SetApprovalDetailsAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching approval details: {Message}", e.Message);
            dispatcher.Dispatch(new SetErrorMessageAction(e.Message));
        }
        finally
        {
            dispatcher.Dispatch(new SetIsDetailsLoadingAction(false));
        }
    }
}
