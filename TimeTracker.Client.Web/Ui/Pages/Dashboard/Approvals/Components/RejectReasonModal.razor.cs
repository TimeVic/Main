using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals.Components;

public partial class RejectReasonModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public EventCallback<string> OnConfirm { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    private EditForm? _editForm;
    private RejectRequest _model = new();

    private async Task OnCancelClicked()
    {
        await OnCancel.InvokeAsync();
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Cancel());
        }
    }

    private Task Submit()
    {
        if (_editForm?.EditContext != null && _editForm.EditContext.Validate())
        {
            return OnValidSubmit();
        }

        return Task.CompletedTask;
    }

    private async Task OnValidSubmit()
    {
        if (string.IsNullOrWhiteSpace(_model.Reason))
        {
            return;
        }

        var reason = _model.Reason.Trim();
        await OnConfirm.InvokeAsync(reason);
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok(reason));
        }
    }
}
