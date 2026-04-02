using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.Client.Effects;

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
