using System.Text.RegularExpressions;
using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public partial class SaveDraftEffect : Effect<SaveNoteDraftAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly ILogger<SaveDraftEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public SaveDraftEffect(
        IApiService apiService,
        IState<NotesState> state,
        ILogger<SaveDraftEffect> logger,
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

    public override async Task HandleAsync(SaveNoteDraftAction action, IDispatcher dispatcher)
    {
        var currentDocument = _state.Value.CurrentDocument;
        var currentContent = _state.Value.CurrentContent;
        if (currentDocument == null || currentContent == null || !currentDocument.IsDraft)
        {
            return;
        }

        var submittedTitle = _state.Value.EditorTitle;
        var submittedMarkdown = _state.Value.EditorMarkdown;
        var isTitleChanged = submittedTitle != currentDocument.Title;
        var isMarkdownChanged = submittedMarkdown != currentContent.MarkdownContent;
        if (!isTitleChanged && !isMarkdownChanged)
        {
            return;
        }

        var isAutoDeriveTitle = !isTitleChanged
            && string.IsNullOrWhiteSpace(currentContent.MarkdownContent)
            && !string.IsNullOrWhiteSpace(submittedMarkdown)
            && DefaultDraftTitleRegex().IsMatch(currentDocument.Title);

        var requestTitle = isAutoDeriveTitle || string.IsNullOrWhiteSpace(submittedTitle)
            ? string.Empty
            : submittedTitle;

        dispatcher.Dispatch(new SetNoteDocumentSavingAction(true));
        dispatcher.Dispatch(new SetNoteSaveErrorAction(false));
        try
        {
            var updatedDraft = await _apiService.NotesUpdateDraftAsync(new UpdateNoteDraftRequest
            {
                NoteId = currentDocument.Id,
                Title = requestTitle,
                MarkdownContent = isMarkdownChanged ? submittedMarkdown : null
            }) ?? throw new InvalidOperationException("Notes update draft response is empty.");

            var updatedContent = new NoteContentDto
            {
                Id = updatedDraft.LastContentId ?? currentContent.Id,
                NoteId = updatedDraft.Id,
                MarkdownContent = isMarkdownChanged ? submittedMarkdown : currentContent.MarkdownContent,
                CreatedAt = updatedDraft.UpdatedAt ?? currentContent.CreatedAt
            };

            dispatcher.Dispatch(new SyncSavedNoteDraftAction(
                updatedDraft,
                updatedContent,
                submittedTitle,
                submittedMarkdown
            ));
        }
        catch (Exception e)
        {
            dispatcher.Dispatch(new SetNoteSaveErrorAction(true));
            _logger.LogError(e, "Failed to save draft note {NoteId}", currentDocument.Id);
            _toastService.ShowError(_localizer["Notes_SaveDraftError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNoteDocumentSavingAction(false));
        }
    }

    [GeneratedRegex(@"^Draft \d+$", RegexOptions.IgnoreCase)]
    private static partial Regex DefaultDraftTitleRegex();
}
