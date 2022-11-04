using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.TimeEntry.Effects;

public class LoadFilteredListEffect: Effect<LoadTimeEntryFilteredListAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IState<TimeEntryState> _state;
    private readonly IApiService _apiService;
    private readonly ILogger<LoadFilteredListEffect> _logger;

    public LoadFilteredListEffect(
        IApiService apiService,
        IState<AuthState> authState,
        IState<TimeEntryState> state,
        ILogger<LoadFilteredListEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _state = state;
        _logger = logger;
    }

    public override async Task HandleAsync(LoadTimeEntryFilteredListAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetTimeEntryIsListLoading(true));
            var response = await _apiService.TimeEntryGetFilteredListAsync(new GetFilteredListRequest()
            {
                WorkspaceId = _authState.Value.Workspace.Id,
                Page = PaginationUtils.CalculatePage(action.Skip),
                Search = _state.Value.Filter.Search,
                ClientId = _state.Value.Filter.ClientId,
                ProjectId = _state.Value.Filter.ProjectId,
                IsBillable = _state.Value.Filter.IsBillable,
                DateFrom = _state.Value.Filter.DateFrom,
                DateTo = _state.Value.Filter.DateTo,
                MemberId = _state.Value.Filter.UserId
            });
            dispatcher.Dispatch(new SetTimeEntryFilteredListItemsAction(response));
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
