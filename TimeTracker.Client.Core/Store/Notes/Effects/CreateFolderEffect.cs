using Fluxor;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Localization;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class CreateFolderEffect : Effect<CreateNoteFolderAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<CreateFolderEffect> _logger;
    private readonly IToastService _toastService;
    private readonly IStringLocalizer<DashboardResource> _localizer;

    public CreateFolderEffect(
        IApiService apiService,
        ILogger<CreateFolderEffect> logger,
        IToastService toastService,
        IStringLocalizer<DashboardResource> localizer
    )
    {
        _apiService = apiService;
        _logger = logger;
        _toastService = toastService;
        _localizer = localizer;
    }

    public override async Task HandleAsync(CreateNoteFolderAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetNotesNodeCreatingAction(true));
        try
        {
            var createdNode = await _apiService.NotesCreateFolderAsync(action.Request);
            if (createdNode != null)
            {
                dispatcher.Dispatch(new ReplaceNoteTreeNodeAction(createdNode));
                dispatcher.Dispatch(new ExpandNoteParentsAction(createdNode.Id));
                dispatcher.Dispatch(new LoadNotesTreeAction(true));
            }

            dispatcher.Dispatch(new SetCreateNoteFolderModalOpenedAction(false));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create notes folder");
            _toastService.ShowError(_localizer["Notes_CreateFolderError"].Value);
        }
        finally
        {
            dispatcher.Dispatch(new SetNotesNodeCreatingAction(false));
        }
    }
}
