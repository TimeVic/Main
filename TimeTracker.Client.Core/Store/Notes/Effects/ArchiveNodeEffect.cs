using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class ArchiveNodeEffect : Effect<ArchiveNoteNodeAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly ILogger<ArchiveNodeEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public ArchiveNodeEffect(
        IApiService apiService,
        IState<NotesState> state,
        ILogger<ArchiveNodeEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _state = state;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(ArchiveNoteNodeAction action, IDispatcher dispatcher)
    {
        var nodeToArchive = _state.Value.NodeToArchive;
        if (nodeToArchive == null)
        {
            return;
        }

        dispatcher.Dispatch(new SetNotesTreeLoadingAction(true));
        try
        {
            await _apiService.NotesArchiveNodeAsync(new ArchiveNoteNodeRequest
            {
                NoteId = nodeToArchive.Id
            });

            var response = await _apiService.NotesGetTreeAsync(new GetNotesTreeRequest());
            var nodes = response?.Nodes?.ToList() ?? [];
            dispatcher.Dispatch(new SetNotesTreeAction(nodes));
            dispatcher.Dispatch(new SetArchiveNoteNodeConfirmationOpenedAction(false));
            if (_state.Value.SelectedNoteId.HasValue && nodes.All(item => item.Id != _state.Value.SelectedNoteId.Value))
            {
                dispatcher.Dispatch(new ClearNoteEditorAction());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to archive note node");
            _toastService.ShowError(_localizer["Notes_ArchiveNodeError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesTreeLoadingAction(false));
        }
    }
}
