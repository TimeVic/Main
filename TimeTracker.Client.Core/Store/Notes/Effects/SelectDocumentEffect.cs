using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class SelectDocumentEffect : Effect<SelectNoteDocumentAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<SelectDocumentEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public SelectDocumentEffect(
        IApiService apiService,
        ILogger<SelectDocumentEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(SelectNoteDocumentAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNoteDocumentLoadingAction(true));
        dispatcher.Dispatch(new SetNoteSaveErrorAction(false));
        try
        {
            var document = await _apiService.NotesGetDocumentAsync(new GetNoteDocumentRequest
            {
                NoteId = action.NoteId
            });
            if (document == null)
            {
                throw new InvalidOperationException("Notes document response is empty.");
            }

            dispatcher.Dispatch(new SetNoteDocumentAction(document, string.IsNullOrWhiteSpace(document.MarkdownContent)));
            dispatcher.Dispatch(new ExpandNoteParentsAction(action.NoteId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load note document {NoteId}", action.NoteId);
            _toastService.ShowError(_localizer["Notes_LoadDocumentError"].Value);
            dispatcher.Dispatch(new ClearNoteEditorAction());
        }
        finally
        {
            dispatcher.Dispatch(new SetNoteDocumentLoadingAction(false));
        }
    }
}
