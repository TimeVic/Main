using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.GoalsTracker.Effects;

public class UpdateItemEffect: Effect<UpdateTrackerItemAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<UpdateItemEffect> _logger;
    private readonly IToastService _toastService;

    public UpdateItemEffect(
        IApiService apiService,
        ILogger<UpdateItemEffect> logger,
        IToastService toastService
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
            _toastService.ShowError("Goal adding error");
            _logger.LogError(e.Message, e);
        }
    }
}
