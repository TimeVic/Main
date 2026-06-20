using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class SaveDocumentEffect : Effect<SaveNoteDocumentAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly ILogger<SaveDocumentEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public SaveDocumentEffect(
        IApiService apiService,
        IState<NotesState> state,
        ILogger<SaveDocumentEffect> logger,
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

    public override async Task HandleAsync(SaveNoteDocumentAction action, IDispatcher dispatcher)
    {
        var currentDocument = _state.Value.CurrentDocument;
        if (currentDocument == null)
        {
            return;
        }

        if (!_state.Value.IsDocumentDirty)
        {
            dispatcher.Dispatch(new SetNoteDocumentAction(currentDocument, false));
            return;
        }

        dispatcher.Dispatch(new SetNoteDocumentSavingAction(true));
        dispatcher.Dispatch(new SetNoteSaveErrorAction(false));
        try
        {
            var document = await _apiService.NotesUpdateDocumentAsync(new UpdateNoteDocumentRequest
            {
                NoteId = currentDocument.Id,
                Title = _state.Value.EditorTitle,
                MarkdownContent = _state.Value.EditorMarkdown,
                Visibility = _state.Value.EditorVisibility
            });
            if (document == null)
            {
                throw new InvalidOperationException("Notes update response is empty.");
            }

            dispatcher.Dispatch(new SetNoteDocumentAction(document, false));
            dispatcher.Dispatch(new UpdateNoteTreeNodeFromDocumentAction(document));
            _toastService.ShowSuccess(_localizer["SavedSuccessfully"].Value);
        }
        catch (Exception e)
        {
            dispatcher.Dispatch(new SetNoteSaveErrorAction(true));
            _logger.LogError(e, "Failed to save note document {NoteId}", currentDocument.Id);
            _toastService.ShowError(_localizer["Notes_SaveDocumentError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNoteDocumentSavingAction(false));
        }
    }
}
