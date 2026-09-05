using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.MemberPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

public partial class UpdateMemberPaymentModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required MemberPaymentDto MemberPayment { get; set; }

    [Inject]
    public IState<MemberPaymentState> _state { get; set; } = default!;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = default!;

    private UpdateRequest model = new();
    private EditForm _form = default!;
    private bool CanCreatePaymentForOtherMembers =>
        SecurityManager.HasPermission(WorkspacePermission.CreateMemberPaymentForOtherMembers);

    protected override async Task OnInitializedAsync()
    {
        model.Fill(MemberPayment);
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (MemberPayment != null && model.MemberPaymentId != MemberPayment.Id)
        {
            model.Fill(MemberPayment);
        }
        base.OnParametersSet();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
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
