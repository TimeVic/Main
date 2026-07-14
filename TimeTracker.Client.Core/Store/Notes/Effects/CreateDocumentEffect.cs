using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class CreateDocumentEffect : Effect<CreateNoteDocumentAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<CreateDocumentEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public CreateDocumentEffect(
        IApiService apiService,
        ILogger<CreateDocumentEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(CreateNoteDocumentAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesNodeCreatingAction(true));
        try
        {
            var document = await _apiService.NotesCreateDocumentAsync(action.Request);
            if (document == null)
            {
                throw new InvalidOperationException("Notes create response is empty.");
            }

            if (!document.LastContentId.HasValue)
            {
                throw new InvalidOperationException("New notes document has no current content.");
            }

            var content = await _apiService.NotesGetContentAsync(new GetNoteContentRequest
            {
                ContentId = document.LastContentId.Value
            });
            if (content == null)
            {
                throw new InvalidOperationException("New notes content response is empty.");
            }

            dispatcher.Dispatch(new ReplaceNoteTreeNodeAction(new NoteTreeNodeDto
            {
                Id = document.Id,
                ParentId = document.ParentId,
                Type = NoteNodeType.Document,
                Title = document.Title,
                LastContentId = document.LastContentId,
                Visibility = document.Visibility,
                UpdatedAt = document.UpdatedAt
            }));
            dispatcher.Dispatch(new SetNoteDocumentAction(document, content, true));
            dispatcher.Dispatch(new ExpandNoteParentsAction(document.Id));
            dispatcher.Dispatch(new LoadNotesTreeAction(true));
            dispatcher.Dispatch(new SetCreateNoteDocumentModalOpenedAction(false));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create note document");
            _toastService.ShowError(_localizer["Notes_CreateNoteError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesNodeCreatingAction(false));
        }
    }
}
