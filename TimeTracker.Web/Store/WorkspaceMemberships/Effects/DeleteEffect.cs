using Fluxor;
using Radzen;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Store.WorkspaceMemberships.Effects;

public class DeleteEffect: Effect<DeleteMemberAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly NotificationService _notificationService;

    public DeleteEffect(
        IApiService apiService,
        ILogger<LoadListEffect> logger,
        NotificationService notificationService
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
            
            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "The member was deleted"
            });
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
