using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class CreateDraftEffect : Effect<CreateNoteDraftAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly IState<AuthState> _authState;
    private readonly NavigationManager _navigationManager;
    private readonly UrlService _urlService;
    private readonly ILogger<CreateDraftEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public CreateDraftEffect(
        IApiService apiService,
        IState<NotesState> state,
        IState<AuthState> authState,
        NavigationManager navigationManager,
        UrlService urlService,
        ILogger<CreateDraftEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _state = state;
        _authState = authState;
        _navigationManager = navigationManager;
        _urlService = urlService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(CreateNoteDraftAction action, IDispatcher dispatcher)
    {
        if (_state.Value.IsCreatingDraft)
        {
            return;
        }

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

            if (action.IsNavigateToNotes && _state.Value.ActiveMode != NoteVisibility.Private)
            {
                dispatcher.Dispatch(new SetNotesActiveModeAction(NoteVisibility.Private));
            }

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

            if (action.IsNavigateToNotes)
            {
                var isSoloWorkspace = _authState.Value.Workspace?.Mode == WorkspaceMode.Solo;
                var targetUrl = isSoloWorkspace
                    ? _urlService.GetDashboardUrl("notes")
                    : $"{_urlService.GetDashboardUrl("notes")}?mode=private";
                var currentRelativeUri = $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";
                if (!string.Equals(currentRelativeUri, targetUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _navigationManager.NavigateTo(targetUrl);
                }
            }
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
