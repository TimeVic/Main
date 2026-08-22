using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals.Components;

public partial class RejectReasonModal
{
    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<string> OnConfirm { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    private LumexModal? _modal;
    private EditForm? _editForm;
    private RejectRequest _model = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (IsOpened)
        {
            _model = new RejectRequest();
        }
    }

    private async Task OnCancelClicked()
    {
        await OnCancel.InvokeAsync();
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

        await OnConfirm.InvokeAsync(_model.Reason.Trim());
    }
}
