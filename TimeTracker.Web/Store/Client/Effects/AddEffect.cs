using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.Client.Effects;

public class AddEffect : Effect<AddAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly ToastService _toastService;

    public AddEffect(
        ApiService apiService,
        ILogger<AddEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(AddAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));
            var response = await _apiService.ClientAddAsync(action.Request);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo("Client has been added");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            _toastService.ShowError("Client adding error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
