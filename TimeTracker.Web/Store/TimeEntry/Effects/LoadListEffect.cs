using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class LoadListEffect: Effect<LoadListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TimeEntryState> _state;
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;

    public LoadListEffect(
        ApiService apiService,
        IState<AuthState> authState,
        IState<TimeEntryState> state,
        ILogger<LoadListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetTimeEntryIsListLoading(true));
            var response = await _apiService.TimeEntryGetListAsync(new GetListRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Page = PaginationUtils.CalculatePage(action.Skip)
            });
            dispatcher.Dispatch(new SetTimeEntryListItemsAction(response));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetTimeEntryIsListLoading(false));
        }
    }
}
