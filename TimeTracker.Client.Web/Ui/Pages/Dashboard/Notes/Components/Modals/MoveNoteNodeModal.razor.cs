using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

public partial class MoveNoteNodeModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public NoteTreeNodeDto? Node { get; set; }

    [Parameter]
    public IReadOnlyList<NoteTreeNodeDto> Nodes { get; set; } = Array.Empty<NoteTreeNodeDto>();

    [Parameter]
    public EventCallback<MoveNoteNodeRequest> OnSubmit { get; set; }

    private Guid? _selectedParentId;
    private Guid? _nodeId;

    protected override void OnParametersSet()
    {
        if (Node == null)
        {
            _nodeId = null;
            return;
        }

        if (_nodeId != Node.Id)
        {
            _nodeId = Node.Id;
            _selectedParentId = null;
        }
    }

    private void OnParentChanged(Guid? parentId)
    {
        _selectedParentId = parentId;
    }

    private async Task Submit()
    {
        if (Node == null)
        {
            return;
        }

        var request = new MoveNoteNodeRequest
        {
            NoteId = Node.Id,
            ParentId = _selectedParentId
        };
        await OnSubmit.InvokeAsync(request);
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok(request));
        }
    }
}
