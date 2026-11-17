using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class DeleteDraftEffect : Effect<DeleteNoteDraftAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<DeleteDraftEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public DeleteDraftEffect(
        IApiService apiService,
        ILogger<DeleteDraftEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(DeleteNoteDraftAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesDraftDeletingAction(true));
        try
        {
            await _apiService.NotesDeleteDraftAsync(new DeleteNoteDraftRequest
            {
                NoteId = action.NoteId
            });

            dispatcher.Dispatch(new RemoveNoteDraftNodeAction(action.NoteId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete draft note {NoteId}", action.NoteId);
            _toastService.ShowError(_localizer["Notes_DeleteDraftError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesDraftDeletingAction(false));
        }
    }
}
