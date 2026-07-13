using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class MoveNodeEffect : Effect<MoveNoteNodeAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<MoveNodeEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public MoveNodeEffect(
        IApiService apiService,
        ILogger<MoveNodeEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(MoveNoteNodeAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesNodeMovingAction(true));
        try
        {
            var updatedNode = await _apiService.NotesMoveNodeAsync(action.Request);
            if (updatedNode == null)
            {
                throw new InvalidOperationException("Notes move response is empty.");
            }

            dispatcher.Dispatch(new ReplaceNoteTreeNodeAction(updatedNode));
            dispatcher.Dispatch(new ExpandNoteParentsAction(updatedNode.Id));
            dispatcher.Dispatch(new SetMoveNoteNodeModalOpenedAction(false));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to move note node {NoteId}", action.Request.NoteId);
            _toastService.ShowError(_localizer["Notes_MoveNodeError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesNodeMovingAction(false));
        }
    }
}
