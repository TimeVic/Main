using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Web.Store.MemberPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

public partial class UpdateMemberPaymentModal
{
    [Parameter]
    public required MemberPaymentDto MemberPayment { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; } = false;

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    public IState<MemberPaymentState> _state { get; set; }

    private UpdateRequest model = new();
    private EditForm _form;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(MemberPayment);
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }

    private void OnChangeClient(ClientDto? client)
    {
        model.ClientId = client?.Id ?? Guid.Empty;
        model.ProjectId = Guid.Empty;
    }

    private void OnProjectSelected(ProjectDto? project)
    {
        model.ProjectId = project?.Id ?? Guid.Empty;
    }

    private void OnMemberSelected(WorkspaceMemberDto? member)
    {
        model.MemberId = member?.Id ?? Guid.Empty;
    }
}
