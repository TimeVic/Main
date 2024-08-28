using Fluxor;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMemberships.Effects;

public class DeleteEffect: Effect<DeleteMemberAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly ToastService _notificationService;

    public DeleteEffect(
        ApiService apiService,
        ILogger<LoadListEffect> logger,
        ToastService notificationService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(DeleteMemberAction action, IDispatcher dispatcher)
    {
        try
        {
            await _apiService.WorkspaceMembershipDeleteAsync(action.Membership.Id);
            dispatcher.Dispatch(new LoadListAction(true));
            
            _notificationService.ShowInfo("The member was deleted");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }
        finally
        {
            dispatcher.Dispatch(new SetIsListLoading(false));
        }
    }
}
