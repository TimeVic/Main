using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components;

public partial class NotesTreeNodeBlock
{
    [Parameter]
    public required NoteTreeNodeModel TreeNode { get; set; }

    [Parameter]
    public int Level { get; set; }

    [Parameter]
    public HashSet<Guid> ExpandedNodeIds { get; set; } = new();

    [Parameter]
    public Guid? SelectedNoteId { get; set; }

    [Parameter]
    public bool IsEditingAllowed { get; set; }

    [Parameter]
    public EventCallback<Guid> ToggleExpandedRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> SelectNodeRequested { get; set; }

    [Parameter]
    public EventCallback<Guid?> CreateFolderRequested { get; set; }

    [Parameter]
    public EventCallback<Guid?> CreateNoteRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> RenameRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> MoveRequested { get; set; }

    [Parameter]
    public EventCallback<NoteTreeNodeDto> ArchiveRequested { get; set; }

    private bool IsFolder => TreeNode.Node.Type == NoteNodeType.Folder;

    private bool IsExpanded => ExpandedNodeIds.Contains(TreeNode.Node.Id);

    private bool IsSelected => SelectedNoteId == TreeNode.Node.Id;

    private string IconClass => IsFolder
        ? "fa-regular fa-folder text-amber-500"
        : "fa-regular fa-note-sticky text-slate-500";

    private string RowClass => SelectedNoteId == TreeNode.Node.Id
        ? "notes-tree-node-row group flex items-center gap-1 rounded-lg border border-blue-200 bg-blue-50 px-1.5 py-0.5 text-sm font-medium text-blue-900"
        : "notes-tree-node-row group flex items-center gap-1 rounded-lg border border-transparent px-1.5 py-0.5 text-sm text-slate-700 hover:border-slate-200 hover:bg-slate-50";

    private async Task OnToggleExpanded()
    {
        if (!IsFolder)
        {
            return;
        }

        await ToggleExpandedRequested.InvokeAsync(TreeNode.Node.Id);
    }

    private async Task OnRowKeyDown(KeyboardEventArgs args)
    {
        if (args.Key is not ("Enter" or " " or "Spacebar"))
        {
            return;
        }

        await OnSelect();
    }

    private async Task OnSelect()
    {
        // Keep the full visual row interactive so border and padding clicks select or expand the node.
        if (IsFolder)
        {
            await ToggleExpandedRequested.InvokeAsync(TreeNode.Node.Id);
            return;
        }

        await SelectNodeRequested.InvokeAsync(TreeNode.Node);
    }
}
