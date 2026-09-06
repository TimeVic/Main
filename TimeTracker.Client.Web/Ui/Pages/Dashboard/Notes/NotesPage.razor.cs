using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Notes;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Services;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes;

public partial class NotesPage
{
    private const int DefaultTreeWidth = 320;
    private const int MinTreeWidth = DefaultTreeWidth - 80;
    private const int MaxTreeWidth = DefaultTreeWidth + 240;

    [Inject]
    private ILogger<NotesPage> Logger { get; set; } = null!;

    [Inject]
    private IState<NotesState> NotesState { get; set; } = null!;

    [Inject]
    private IState<AuthState> AuthState { get; set; } = null!;

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    private TimeTracker.Client.Web.Services.UI.IModalDialogProviderService ModalDialogService { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "noteId")]
    public Guid? InitialNoteId { get; set; }

    private bool _isInitialized;
    private Guid? _lastInitialNoteId;

    private NotesState State => NotesState.Value;

    private bool IsSoloWorkspace => AuthState.Value.Workspace?.Mode == WorkspaceMode.Solo;

    private NoteVisibility ActiveMode => IsSoloWorkspace ? NoteVisibility.Private : State.ActiveMode;

    private IReadOnlyList<NoteTreeNodeModel> TreeNodes => NotesTreeBuilder.BuildNotesTree(
        State.FlatNodes.Where(item => item.Visibility == ActiveMode).ToList(),
        Logger
    );

    private string? TreeError => string.IsNullOrWhiteSpace(State.TreeErrorLocalizationKey)
        ? null
        : DashboardLocalizer[State.TreeErrorLocalizationKey].Value;

    private bool IsDocumentDirty => State.IsDocumentDirty;

    private bool CanManageTree => IsSoloWorkspace
        || State.ActiveMode == NoteVisibility.Private
        || SecurityManager.HasPermission(WorkspacePermission.UpdateWorkspace);

    private bool CanEditDocument => IsSoloWorkspace
        || State.ActiveMode == NoteVisibility.Private
        || AuthState.Value.Workspace?.Mode == WorkspaceMode.Team
        || SecurityManager.HasPermission(WorkspacePermission.UpdateWorkspace);

    private Task SetMode(NoteVisibility mode)
    {
        Dispatcher.Dispatch(new SetNotesActiveModeAction(mode));
        return Task.CompletedTask;
    }

    private string GetModeButtonClass(NoteVisibility mode)
    {
        var isActive = State.ActiveMode == mode;
        return isActive
            ? "inline-flex items-center rounded-lg bg-white px-3.5 py-1.5 text-xs font-semibold text-slate-900 shadow-sm transition"
            : "inline-flex items-center rounded-lg px-3.5 py-1.5 text-xs font-medium text-slate-600 hover:text-slate-900 transition";
    }

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
        if (IsSoloWorkspace && State.ActiveMode != NoteVisibility.Private)
        {
            Dispatcher.Dispatch(new SetNotesActiveModeAction(NoteVisibility.Private));
        }

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
        Dispatcher.Dispatch(new LoadNotesTreeAction(
            InitialNoteId: InitialNoteId,
            Visibility: IsSoloWorkspace ? null : ActiveMode
        ));
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

    private Task OnAttachmentsChanged(ICollection<StoredFileDto> attachments)
    {
        Dispatcher.Dispatch(new SetNoteDocumentAttachmentsAction(attachments));
        return Task.CompletedTask;
    }

    private Task StartDocumentEditing()
    {
        Dispatcher.Dispatch(new StartNoteDocumentEditingAction());
        return Task.CompletedTask;
    }

    private Task CancelDocumentEditing()
    {
        Dispatcher.Dispatch(new CancelNoteDocumentEditingAction());
        return Task.CompletedTask;
    }

    private Task SaveDocument()
    {
        Dispatcher.Dispatch(new SaveNoteDocumentAction());
        return Task.CompletedTask;
    }

    private async Task OpenCreateFolderModal(Guid? parentId)
    {
        var visibility = IsSoloWorkspace
            ? NoteVisibility.Private
            : (parentId.HasValue
                ? State.FlatNodes.FirstOrDefault(item => item.Id == parentId.Value)?.Visibility ?? State.ActiveMode
                : State.ActiveMode);

        await ModalDialogService.ShowCreateFolderModal(
            parentId: parentId,
            visibility: visibility,
            onSubmit: EventCallback.Factory.Create<CreateNoteFolderRequest>(this, CreateFolder)
        );
    }

    private async Task OpenCreateNoteModal(Guid? parentId)
    {
        var visibility = IsSoloWorkspace
            ? NoteVisibility.Private
            : (parentId.HasValue
                ? State.FlatNodes.FirstOrDefault(item => item.Id == parentId.Value)?.Visibility ?? State.ActiveMode
                : State.ActiveMode);

        await ModalDialogService.ShowCreateNoteModal(
            parentId: parentId,
            visibility: visibility,
            onSubmit: EventCallback.Factory.Create<CreateNoteDocumentRequest>(this, CreateDocument)
        );
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

    private async Task OpenRenameModal(NoteTreeNodeDto node)
    {
        await ModalDialogService.ShowRenameNoteNodeModal(
            node: node,
            onSubmit: EventCallback.Factory.Create<RenameNoteNodeRequest>(this, RenameNode)
        );
    }

    private Task RenameNode(RenameNoteNodeRequest request)
    {
        Dispatcher.Dispatch(new RenameNoteNodeAction(request));
        return Task.CompletedTask;
    }

    private async Task OpenMoveModal(NoteTreeNodeDto node)
    {
        await ModalDialogService.ShowMoveNoteNodeModal(
            node: node,
            nodes: State.FlatNodes,
            onSubmit: EventCallback.Factory.Create<MoveNoteNodeRequest>(this, MoveNode)
        );
    }

    private Task MoveNode(MoveNoteNodeRequest request)
    {
        Dispatcher.Dispatch(new MoveNoteNodeAction(request));
        return Task.CompletedTask;
    }

    private async Task OpenArchiveConfirmation(NoteTreeNodeDto node)
    {
        var confirmed = await ModalDialogService.ShowConfirmationAsync(
            DashboardLocalizer["Notes_ArchiveConfirmSubtitle"].Value,
            DashboardLocalizer["Notes_ArchiveConfirmTitle"].Value,
            confirmText: DashboardLocalizer["Archive"].Value
        );
        if (confirmed)
        {
            Dispatcher.Dispatch(new OpenArchiveNoteNodeConfirmationAction(node));
            Dispatcher.Dispatch(new ArchiveNoteNodeAction());
        }
    }
}
