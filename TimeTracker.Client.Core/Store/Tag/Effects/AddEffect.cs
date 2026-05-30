using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Tag.Effects;

public class AddEffect : Effect<AddAction>
{
    private readonly IApiService _apiService;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<AddEffect> _logger;
    private readonly IToastService _toastService;

    public AddEffect(
        IApiService apiService,
        IState<AuthState> authState,
        ILogger<AddEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _authState = authState;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(AddAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));

            var response = await _apiService.TagAddAsync(action.Request);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo("Tag has been added");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _toastService.ShowError("Tag adding error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
