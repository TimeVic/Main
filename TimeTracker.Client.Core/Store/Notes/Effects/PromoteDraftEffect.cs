using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class PromoteDraftEffect : Effect<PromoteNoteDraftAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly ILogger<PromoteDraftEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public PromoteDraftEffect(
        IApiService apiService,
        IState<NotesState> state,
        ILogger<PromoteDraftEffect> logger,
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

    public override async Task HandleAsync(PromoteNoteDraftAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesDraftPromotingAction(true));
        try
        {
            var request = action.Request;
            if (request.MarkdownContent == null
                && _state.Value.CurrentDocument?.Id == request.NoteId
                && _state.Value.CurrentContent != null
                && _state.Value.EditorMarkdown != _state.Value.CurrentContent.MarkdownContent)
            {
                request = new PromoteNoteDraftRequest
                {
                    NoteId = request.NoteId,
                    Title = request.Title,
                    Visibility = request.Visibility,
                    ParentId = request.ParentId,
                    SortOrder = request.SortOrder,
                    MarkdownContent = _state.Value.EditorMarkdown
                };
            }

            var promotedDocument = await _apiService.NotesPromoteDraftAsync(request)
                ?? throw new InvalidOperationException("Notes promote draft response is empty.");

            if (!promotedDocument.LastContentId.HasValue)
            {
                throw new InvalidOperationException("Promoted note has no current content.");
            }

            var content = await _apiService.NotesGetContentAsync(new GetNoteContentRequest
            {
                ContentId = promotedDocument.LastContentId.Value
            }) ?? throw new InvalidOperationException("Promoted note content response is empty.");

            dispatcher.Dispatch(new RemoveNoteDraftNodeAction(promotedDocument.Id));

            if (_state.Value.ActiveMode != promotedDocument.Visibility)
            {
                dispatcher.Dispatch(new SetNotesActiveModeAction(promotedDocument.Visibility));
            }
            else
            {
                dispatcher.Dispatch(new ReplaceNoteTreeNodeAction(new NoteTreeNodeDto
                {
                    Id = promotedDocument.Id,
                    ParentId = promotedDocument.ParentId,
                    Type = NoteNodeType.Document,
                    Title = promotedDocument.Title,
                    LastContentId = promotedDocument.LastContentId,
                    Visibility = promotedDocument.Visibility,
                    IsDraft = false,
                    UpdatedAt = promotedDocument.UpdatedAt
                }));
            }

            dispatcher.Dispatch(new SetNoteDocumentAction(promotedDocument, content, false));
            dispatcher.Dispatch(new LoadNotesTreeAction(
                IsReload: true,
                InitialNoteId: promotedDocument.Id,
                Visibility: promotedDocument.Visibility
            ));
            _toastService.ShowSuccess(_localizer["Notes_PromoteDraftSuccess"].Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to promote draft note {NoteId}", action.Request.NoteId);
            _toastService.ShowError(_localizer["Notes_PromoteDraftError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesDraftPromotingAction(false));
        }
    }
}
