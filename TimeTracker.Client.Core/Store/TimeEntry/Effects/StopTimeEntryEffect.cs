using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Core.Store.TimeEntry.Effects;

public class StopTimeEntryEffect: Effect<StopActiveTimeEntryAction>
{
    private readonly IApiService _apiService;
    private readonly NavigationManager _navigationManager;
    private readonly UrlService _urlService;
    private readonly ILogger<StopTimeEntryEffect> _logger;

    public StopTimeEntryEffect(
        IApiService apiService,
        NavigationManager navigationManager,
        UrlService urlService,
        ILogger<StopTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _navigationManager = navigationManager;
        _urlService = urlService;
        _logger = logger;
    }

    public override async Task HandleAsync(StopActiveTimeEntryAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(true));
            var stoppedTimeEntry = await _apiService.TimeEntryStopAsync(new StopRequest()
            {
                EndTime = DateTime.UtcNow
            });
            if (stoppedTimeEntry?.Task != null)
            {
                dispatcher.Dispatch(new UpdateListItemsAction(new[] { stoppedTimeEntry.Task }));
            }

            dispatcher.Dispatch(new SetActiveTimeEntryAction(null));
            AddStoppedTimeEntryToListIfTimeEntriesPageIsOpen(stoppedTimeEntry, dispatcher);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsTimeEntryProcessingAction(false));
        }
    }

    private void AddStoppedTimeEntryToListIfTimeEntriesPageIsOpen(
        TimeEntryDto? stoppedTimeEntry,
        IDispatcher dispatcher
    )
    {
        if (stoppedTimeEntry == null)
        {
            return;
        }

        var currentPath = _navigationManager.GetPath().TrimEnd('/');
        var timeEntriesPath = _urlService.GetDashboardUrl().TrimEnd('/');
        if (!string.Equals(currentPath, timeEntriesPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        dispatcher.Dispatch(new AddTimeEntryToListAction(stoppedTimeEntry));
    }
}
