using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Dashboard.Effects;

public class FetchCountersEffect : Effect<FetchCountersAction>
{
    private readonly IApiService _apiService;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<FetchCountersEffect> _logger;

    public FetchCountersEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<FetchCountersEffect> logger
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(FetchCountersAction action, IDispatcher dispatcher)
    {
        if (_authState.Value.Workspace == null)
        {
            return;
        }

        try
        {
            var response = await _apiService.DashboardGetCountersAsync();
            if (response != null)
            {
                dispatcher.Dispatch(new SetCountersAction(response.Counters));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load dashboard counters: {Message}", e.Message);
        }
    }
}
