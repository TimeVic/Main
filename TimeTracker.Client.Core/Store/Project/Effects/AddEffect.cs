using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Project.Effects;

public class AddEffect : Effect<AddAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<AddEffect> _logger;
    private readonly IToastService _toastService;

    public AddEffect(
        IApiService apiService,
        ILogger<AddEffect> logger,
        IToastService toastService
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
            var response = await _apiService.ProjectAddAsync(action.Request);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                _toastService.ShowInfo("Project has been added");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            _toastService.ShowError("Project adding error");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
