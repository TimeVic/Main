using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class MoveNoteFolderSelect
{
    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> Nodes { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public Guid? SelectedFolderId { get; set; }

    [Parameter]
    public EventCallback<Guid?> SelectedFolderIdChanged { get; set; }

    private IReadOnlyList<NoteTreeNodeDto> Folders => Nodes
        .Where(item => item.Type == NoteNodeType.Folder && item.ParentId == SelectedFolderId)
        .OrderBy(item => item.SortOrder)
        .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    private IReadOnlyList<string> Path
    {
        get
        {
            var foldersById = Nodes
                .Where(item => item.Type == NoteNodeType.Folder)
                .ToDictionary(item => item.Id);
            var path = new List<string>();
            var visitedFolderIds = new HashSet<Guid>();
            var folderId = SelectedFolderId;

            while (folderId.HasValue && visitedFolderIds.Add(folderId.Value) && foldersById.TryGetValue(folderId.Value, out var folder))
            {
                path.Add(folder.Title);
                folderId = folder.ParentId;
            }

            path.Reverse();
            path.Insert(0, DashboardLocalizer["Notes_RootFolder"].Value);
            return path;
        }
    }

    private Task OpenFolder(Guid folderId)
    {
        return SelectedFolderIdChanged.InvokeAsync(folderId);
    }

    private Task GoBack()
    {
        var parentId = Nodes
            .FirstOrDefault(item => item.Id == SelectedFolderId)?.ParentId;
        return SelectedFolderIdChanged.InvokeAsync(parentId);
    }
}
