using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.TimeEntry.Effects;

namespace TimeTracker.Client.Core.Store.Messaging.Channels.Effects;

public class CreateEffect: Effect<CreateChannelAction>
{
    private readonly IState<AuthState> _authState;
    private readonly IApiService _apiService;
    private readonly IToastService _toastService;
    private readonly ILogger<SetTimeEntryEffect> _logger;

    public CreateEffect(
        IApiService apiService,
        IToastService toastService,
        IState<AuthState> authState,
        ILogger<SetTimeEntryEffect> logger
    )
    {
        _apiService = apiService;
        _toastService = toastService;
        _authState = authState;
        _logger = logger;
    }

    public override async Task HandleAsync(CreateChannelAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.MessagingChannelCreateAsync(
                _authState.Value.Workspace!.Id,
                action.Slug
            );
        }
        catch (Exception e)
        {
            _toastService.ShowError(e.Message);
            _logger.LogError(e.Message, e);
        }
    }
}
