using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Client.Effects;

public class UpdateEffect: Effect<UpdateAction>
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

    public override async Task HandleAsync(UpdateAction action, IDispatcher dispatcher)
    {
        try
        {
            dispatcher.Dispatch(new SetIsListLoading(true));
            var response = await _apiService.ClientUpdateAsync(new UpdateRequest()
            {
                Id = action.Client.Id,
                Name = action.Client.Name
            });
            dispatcher.Dispatch(new SetListItemAction(response));
            _toastService.ShowSuccess("Updated Successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            _toastService.ShowSuccess($"Client update error: {e.Message}");
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
