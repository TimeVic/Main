using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Store.Project.Effects;

public class UpdateEffect: Effect<UpdateAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly ToastService _toastService;

    public UpdateEffect(
        ApiService apiService,
        ILogger<UpdateEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));
            var response = await _apiService.ProjectUpdateAsync(action.Request);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo("Project updated successfully");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _toastService.ShowError($"Project update error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
