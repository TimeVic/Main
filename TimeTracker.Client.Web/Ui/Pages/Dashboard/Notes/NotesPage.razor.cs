using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Store.Notes;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Services;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes;

public partial class NotesPage
{
    [Inject]
    private ILogger<NotesPage> Logger { get; set; } = null!;

    [Inject]
    private IState<NotesState> NotesState { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "noteId")]
    public Guid? InitialNoteId { get; set; }

    private bool _isInitialized;
    private Guid? _lastInitialNoteId;

    private NotesState State => NotesState.Value;

    private IReadOnlyList<NoteTreeNodeModel> TreeNodes => NotesTreeBuilder.BuildNotesTree(State.FlatNodes, Logger);

    private string? TreeError => string.IsNullOrWhiteSpace(State.TreeErrorLocalizationKey)
        ? null
        : DashboardLocalizer[State.TreeErrorLocalizationKey].Value;

    private bool IsDocumentDirty => State.IsDocumentDirty;

    private string SaveStateLabel
    {
        get
        {
            if (State.IsDocumentSaving)
                return DashboardLocalizer["Saving"].Value;
            if (State.IsSaveError)
                return DashboardLocalizer["ErrorSaving"].Value;
            return IsDocumentDirty
                ? DashboardLocalizer["UnsavedChanges"].Value
                : DashboardLocalizer["Saved"].Value;
        }
    }

    private string SaveStateClass
    {
        get
        {
            if (State.IsDocumentSaving)
                return "rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700";
            if (State.IsSaveError)
                return "rounded-full bg-red-50 px-3 py-1 text-xs font-semibold text-red-700";
            return IsDocumentDirty
                ? "rounded-full bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700"
                : "rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700";
        }
    }

    protected override void OnParametersSet()
    {
        if (_isInitialized && InitialNoteId == _lastInitialNoteId)
        {
            return;
        }

        _isInitialized = true;
        _lastInitialNoteId = InitialNoteId;
        LoadTree();
    }

    private void LoadTree()
    {
        _lastInitialNoteId = InitialNoteId;
        Dispatcher.Dispatch(new LoadNotesTreeAction(InitialNoteId: InitialNoteId));
    }

    private Task ToggleExpanded(Guid noteId)
    {
        Dispatcher.Dispatch(new ToggleNoteExpandedAction(noteId));
        return Task.CompletedTask;
    }

    private Task SelectDocument(NoteTreeNodeDto node)
    {
        if (node.Type == NoteNodeType.Document)
        {
            Dispatcher.Dispatch(new SelectNoteDocumentAction(node.Id));
        }

        return Task.CompletedTask;
    }

    private Task OnTitleChanged(string title)
    {
        Dispatcher.Dispatch(new SetNoteEditorTitleAction(title));
        return Task.CompletedTask;
    }

    private Task OnMarkdownChanged(string markdown)
    {
        Dispatcher.Dispatch(new SetNoteEditorMarkdownAction(markdown));
        return Task.CompletedTask;
    }

    private Task OnVisibilityChanged(NoteVisibility visibility)
    {
        Dispatcher.Dispatch(new SetNoteEditorVisibilityAction(visibility));
        return Task.CompletedTask;
    }

    private Task StartDocumentEditing()
    {
        Dispatcher.Dispatch(new StartNoteDocumentEditingAction());
        return Task.CompletedTask;
    }

    private Task SaveDocument()
    {
        Dispatcher.Dispatch(new SaveNoteDocumentAction());
        return Task.CompletedTask;
    }

    private Task OpenCreateFolderModal(Guid? parentId)
    {
        Dispatcher.Dispatch(new OpenCreateNoteFolderModalAction(parentId));
        return Task.CompletedTask;
    }

    private Task OpenCreateNoteModal(Guid? parentId)
    {
        Dispatcher.Dispatch(new OpenCreateNoteDocumentModalAction(parentId));
        return Task.CompletedTask;
    }

    private Task CreateFolder(CreateNoteFolderRequest request)
    {
        Dispatcher.Dispatch(new CreateNoteFolderAction(request));
        return Task.CompletedTask;
    }

    private Task CreateDocument(CreateNoteDocumentRequest request)
    {
        Dispatcher.Dispatch(new CreateNoteDocumentAction(request));
        return Task.CompletedTask;
    }

    private Task OpenRenameModal(NoteTreeNodeDto node)
    {
        Dispatcher.Dispatch(new OpenRenameNoteNodeModalAction(node));
        return Task.CompletedTask;
    }

    private Task RenameNode(RenameNoteNodeRequest request)
    {
        Dispatcher.Dispatch(new RenameNoteNodeAction(request));
        return Task.CompletedTask;
    }

    private Task OpenArchiveConfirmation(NoteTreeNodeDto node)
    {
        Dispatcher.Dispatch(new OpenArchiveNoteNodeConfirmationAction(node));
        return Task.CompletedTask;
    }

    private Task ArchiveNode()
    {
        Dispatcher.Dispatch(new ArchiveNoteNodeAction());
        return Task.CompletedTask;
    }

    private Task OnCreateFolderModalChanged(bool isOpened)
    {
        Dispatcher.Dispatch(new SetCreateNoteFolderModalOpenedAction(isOpened));
        return Task.CompletedTask;
    }

    private Task OnCreateNoteModalChanged(bool isOpened)
    {
        Dispatcher.Dispatch(new SetCreateNoteDocumentModalOpenedAction(isOpened));
        return Task.CompletedTask;
    }

    private Task OnRenameModalChanged(bool isOpened)
    {
        Dispatcher.Dispatch(new SetRenameNoteNodeModalOpenedAction(isOpened));
        return Task.CompletedTask;
    }

    private void CloseArchiveConfirmation()
    {
        Dispatcher.Dispatch(new SetArchiveNoteNodeConfirmationOpenedAction(false));
    }
}
