using Fluxor;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.WorkspaceMembers.Effects;

public class DeleteEffect: Effect<DeleteMemberAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<LoadListEffect> _logger;
    private readonly IToastService _notificationService;

    public DeleteEffect(
        IApiService apiService,
        ILogger<LoadListEffect> logger,
        IToastService notificationService
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
            await _apiService.WorkspaceMemberDeleteAsync(action.Member.Id);
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
