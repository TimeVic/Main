using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.TimeEntry.Effects;

namespace TimeTracker.Web.Store.Messaging.Channels.Effects;

public class CreateEffect: Effect<CreateChannelAction>
{
    private readonly IState<AuthState> _authState;
    private readonly ApiService _apiService;
    private readonly ToastService _toastService;
    private readonly ILogger<SetTimeEntryEffect> _logger;

    public CreateEffect(
        ApiService apiService,
        ToastService toastService,
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
