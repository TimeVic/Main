using LumexUI;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class MoveNoteNodeModal
{
    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public NoteTreeNodeDto? Node { get; set; }

    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> Nodes { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Parameter]
    public EventCallback<MoveNoteNodeRequest> OnSubmit { get; set; }

    private Guid? _selectedParentId;
    private Guid? _nodeId;
    private bool _wasOpened;
    private LumexModal _modal = null!;

    protected override void OnParametersSet()
    {
        if (Node == null)
        {
            _nodeId = null;
            _wasOpened = false;
            return;
        }

        if (_nodeId != Node.Id || !_wasOpened)
        {
            _nodeId = Node.Id;
            _selectedParentId = null;
        }

        _wasOpened = IsOpened;
    }

    private void OnParentChanged(Guid? parentId)
    {
        _selectedParentId = parentId;
    }

    private Task Submit()
    {
        return Node == null
            ? Task.CompletedTask
            : OnSubmit.InvokeAsync(new MoveNoteNodeRequest
            {
                NoteId = Node.Id,
                ParentId = _selectedParentId
            });
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
    }
}
