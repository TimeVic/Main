using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.GoalsTracker.Effects;

public class UpdateItemEffect: Effect<UpdateTrackerItemAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<UpdateItemEffect> _logger;
    private readonly ToastService _toastService;

    public UpdateItemEffect(
        ApiService apiService,
        ILogger<UpdateItemEffect> logger,
        ToastService toastService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
    }

    public override async Task HandleAsync(UpdateTrackerItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var updatedItem = await _apiService.GoalsTrackerUpdateItemAsync(action.Request);
            dispatcher.Dispatch(new SetGoalsTrackerItemAction(updatedItem));
        }
        catch (Exception e)
        {
            await _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
