using Fluxor;
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

public class LoadTreeEffect : Effect<LoadNotesTreeAction>
{
    private readonly IApiService _apiService;
    private readonly IState<NotesState> _state;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<LoadTreeEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public LoadTreeEffect(
        IApiService apiService,
        IState<NotesState> state,
        IState<AuthState> authState,
        ILogger<LoadTreeEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _state = state;
        _authState = authState;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(LoadNotesTreeAction action, IDispatcher dispatcher)
    {
        var isInitialNoteMissing = action.InitialNoteId.HasValue
                                   && _state.Value.FlatNodes.All(item => item.Id != action.InitialNoteId.Value);
        var isLoadRequired = action.IsReload || !_state.Value.IsTreeLoaded || isInitialNoteMissing;
        if (!isLoadRequired)
        {
            SelectInitialNoteIfNeeded(action.InitialNoteId, dispatcher);
            return;
        }

        dispatcher.Dispatch(new SetNotesTreeLoadingAction(true));
        dispatcher.Dispatch(new SetNotesTreeErrorAction(null));
        try
        {
            var isSoloWorkspace = _authState.Value.Workspace?.Mode == WorkspaceMode.Solo;
            NoteVisibility? visibility = isSoloWorkspace ? null : (action.Visibility ?? _state.Value.ActiveMode);
            var response = await _apiService.NotesGetTreeAsync(new GetNotesTreeRequest
            {
                Visibility = visibility
            });
            var nodes = response?.Nodes?.ToList() ?? [];
            dispatcher.Dispatch(new SetNotesTreeAction(nodes));
            SelectInitialNoteIfNeeded(action.InitialNoteId, nodes, dispatcher);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load notes tree");
            dispatcher.Dispatch(new SetNotesTreeErrorAction("Notes_LoadTreeError"));
            _toastService.ShowError(_localizer["Notes_LoadTreeError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesTreeLoadingAction(false));
        }
    }

    private void SelectInitialNoteIfNeeded(Guid? initialNoteId, IDispatcher dispatcher)
    {
        SelectInitialNoteIfNeeded(initialNoteId, _state.Value.FlatNodes, dispatcher);
    }

    private void SelectInitialNoteIfNeeded(
        Guid? initialNoteId,
        IReadOnlyList<NoteTreeNodeDto> nodes,
        IDispatcher dispatcher
    )
    {
        if (!initialNoteId.HasValue || _state.Value.SelectedNoteId == initialNoteId.Value)
        {
            return;
        }

        var node = nodes.FirstOrDefault(item => item.Id == initialNoteId.Value);
        if (node?.Type != NoteNodeType.Document)
        {
            return;
        }

        dispatcher.Dispatch(new SelectNoteDocumentAction(initialNoteId.Value));
    }
}
