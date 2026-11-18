using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class CreateDraftEffect : Effect<CreateNoteDraftAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<CreateDraftEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public CreateDraftEffect(
        IApiService apiService,
        ILogger<CreateDraftEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(CreateNoteDraftAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesDraftCreatingAction(true));
        try
        {
            var draft = await _apiService.NotesCreateDraftAsync(new CreateNoteDraftRequest());
            if (draft == null)
            {
                throw new InvalidOperationException("Notes create draft response is empty.");
            }

            if (!draft.LastContentId.HasValue)
            {
                throw new InvalidOperationException("New draft has no current content.");
            }

            var content = new NoteContentDto
            {
                Id = draft.LastContentId.Value,
                NoteId = draft.Id,
                MarkdownContent = string.Empty,
                CreatedAt = draft.CreatedAt
            };

            dispatcher.Dispatch(new ReplaceNoteDraftNodeAction(new NoteTreeNodeDto
            {
                Id = draft.Id,
                ParentId = null,
                Type = NoteNodeType.Document,
                Title = draft.Title,
                LastContentId = draft.LastContentId,
                Visibility = draft.Visibility,
                IsDraft = true,
                UpdatedAt = draft.UpdatedAt ?? draft.CreatedAt
            }));
            dispatcher.Dispatch(new SetNoteDocumentAction(draft, content, true));
            dispatcher.Dispatch(new RequestDraftEditorFocusAction());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create draft note");
            _toastService.ShowError(_localizer["Notes_CreateDraftError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesDraftCreatingAction(false));
        }
    }
}
