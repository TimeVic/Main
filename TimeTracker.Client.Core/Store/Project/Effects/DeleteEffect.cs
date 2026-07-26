using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Project.Effects;

public class DeleteEffect : Effect<DeleteItemAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public DeleteEffect(
        IApiService apiService,
        ILogger<DeleteEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(DeleteItemAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsSavingAction(true));
            await _apiService.ProjectDeleteAsync(action.Project.Id);
            dispatcher.Dispatch(new DeleteListItemAction(action.Project.Id));
            dispatcher.Dispatch(new LoadListAction(true));
            _toastService.ShowInfo(_localizer["ProjectsBlock_DeleteProjectSuccess"].Value);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Project deletion failed");
            _toastService.ShowError(_localizer["ProjectsBlock_DeleteProjectError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsSavingAction(false));
        }
    }
}
