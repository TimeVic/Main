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
        var currentContent = _state.Value.CurrentContent;
        if (currentDocument == null || currentContent == null)
        {
            return;
        }

        if (!_state.Value.IsDocumentDirty)
        {
            dispatcher.Dispatch(new SetNoteDocumentAction(currentDocument, currentContent, false));
            return;
        }

        dispatcher.Dispatch(new SetNoteDocumentSavingAction(true));
        dispatcher.Dispatch(new SetNoteSaveErrorAction(false));
        try
        {
            var document = currentDocument;
            var content = currentContent;
            var isMetadataChanged = _state.Value.EditorTitle != currentDocument.Title
                || _state.Value.EditorVisibility != currentDocument.Visibility;
            if (isMetadataChanged)
            {
                document = await _apiService.NotesUpdateDocumentAsync(new UpdateNoteDocumentRequest
                {
                    NoteId = currentDocument.Id,
                    Title = _state.Value.EditorTitle,
                    Visibility = _state.Value.EditorVisibility
                }) ?? throw new InvalidOperationException("Notes update response is empty.");
            }

            if (_state.Value.EditorMarkdown != currentContent.MarkdownContent)
            {
                content = await _apiService.NotesUpdateContentAsync(new UpdateNoteContentRequest
                {
                    NoteId = currentDocument.Id,
                    MarkdownContent = _state.Value.EditorMarkdown
                }) ?? throw new InvalidOperationException("Notes content update response is empty.");

                document.LastContentId = content.Id;
                document.UpdatedAt = content.CreatedAt;
            }

            dispatcher.Dispatch(new SetNoteDocumentAction(document, content, false));
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
