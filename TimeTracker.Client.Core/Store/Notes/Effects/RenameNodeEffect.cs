using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class RenameNodeEffect : Effect<RenameNoteNodeAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<RenameNodeEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public RenameNodeEffect(
        IApiService apiService,
        ILogger<RenameNodeEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(RenameNoteNodeAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesNodeRenamingAction(true));
        try
        {
            var updatedNode = await _apiService.NotesRenameNodeAsync(action.Request);
            if (updatedNode != null)
            {
                dispatcher.Dispatch(new ReplaceNoteTreeNodeAction(updatedNode));
            }

            dispatcher.Dispatch(new SetRenameNoteNodeModalOpenedAction(false));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to rename note node {NoteId}", action.Request.NoteId);
            _toastService.ShowError(_localizer["Notes_RenameNodeError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesNodeRenamingAction(false));
        }
    }
}
