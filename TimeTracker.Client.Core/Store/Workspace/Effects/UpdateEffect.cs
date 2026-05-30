using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Workspace.Effects;

public class UpdateEffect: Effect<UpdateWorkspaceAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UpdateEffect> _logger;
    private readonly IToastService _toastService;

    public UpdateEffect(
        IApiService apiService,
        ILogger<UpdateEffect> logger,
        IToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoading(true));

            var response = await _apiService.WorkspaceUpdateAsync(action.Model);
            if (response != null)
            {
                dispatcher.Dispatch(new SetListItemAction(response));
                dispatcher.Dispatch(new SetWorkspaceAction(response));
                _toastService.ShowInfo($"Workspace updated.");
            }
        }
        catch (Exception e)
        {
            _toastService.ShowError("Workspace saving error");
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
