using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Web.Ui.Pages.Dashboard.Notes.Models;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Notes.Components;

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
    public EventCallback<NoteTreeNodeDto> ArchiveRequested { get; set; }

    private bool IsFolder => TreeNode.Node.Type == NoteNodeType.Folder;

    private bool IsExpanded => ExpandedNodeIds.Contains(TreeNode.Node.Id);

    private string IconClass => IsFolder
        ? "fa-regular fa-folder text-amber-500"
        : "fa-regular fa-note-sticky text-slate-500";

    private string IndentStyle => $"padding-left: {Math.Min(Level * 1.25, 6):0.##}rem;";

    private string ChildEmptyIndentStyle => $"padding-left: {Math.Min((Level + 1) * 1.25 + 2, 8):0.##}rem;";

    private string RowClass => SelectedNoteId == TreeNode.Node.Id
        ? "group flex items-center gap-1 rounded-xl border border-blue-200 bg-blue-50 px-2 py-2 text-sm font-medium text-blue-900"
        : "group flex items-center gap-1 rounded-xl border border-transparent px-2 py-2 text-sm text-slate-700 hover:border-slate-200 hover:bg-slate-50";

    private async Task OnToggleExpanded()
    {
        if (!IsFolder)
        {
            return;
        }

        await ToggleExpandedRequested.InvokeAsync(TreeNode.Node.Id);
    }

    private async Task OnSelect()
    {
        if (IsFolder)
        {
            await ToggleExpandedRequested.InvokeAsync(TreeNode.Node.Id);
            return;
        }

        await SelectNodeRequested.InvokeAsync(TreeNode.Node);
    }
}
