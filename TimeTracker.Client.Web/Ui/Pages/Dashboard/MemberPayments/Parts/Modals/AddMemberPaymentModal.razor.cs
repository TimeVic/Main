using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.MemberPayments;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

public partial class AddMemberPaymentModal : IDisposable
{   
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public Guid InitialMemberId { get; set; }

    [Parameter]
    public decimal? InitialAmount { get; set; }

    [Parameter]
    public Guid InitialProjectId { get; set; }
    
    [Inject]
    public ILogger<AddMemberPaymentModal> _logger { get; set; } = default!;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = default!;

    [Inject]
    public IState<WorkspaceMembersState> WorkspaceMembersState { get; set; } = default!;
    
    private AddRequest model = default!;
    private EditForm _form = default!;
    private bool CanCreatePaymentForOtherMembers =>
        SecurityManager.HasPermission(WorkspacePermission.CreateMemberPaymentForOtherMembers);

    protected override async Task OnInitializedAsync()
    {
        InitModel();
        WorkspaceMembersState.StateChanged += OnWorkspaceMembersStateChanged;
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (InitialMemberId != Guid.Empty)
        {
            model.MemberId = InitialMemberId;
        }
        if (InitialAmount.HasValue && InitialAmount.Value > 0)
        {
            model.Amount = InitialAmount.Value;
        }
        if (InitialProjectId != Guid.Empty)
        {
            model.ProjectId = InitialProjectId;
        }

        base.OnParametersSet();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        Dispatcher.Dispatch(new AddMemberPaymentAction(model));
        InitModel();
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }

    private void InitModel()
    {
        model = new AddRequest
        {
            PaymentTime = UserDateTimeProviderService.GetCurrentTime().DateTime
        };
        SetCurrentUserMemberAsDefault();
    }

    private void OnWorkspaceMembersStateChanged(object? sender, EventArgs args)
    {
        if (model.MemberId != Guid.Empty)
        {
            return;
        }

        SetCurrentUserMemberAsDefault();
        InvokeAsync(StateHasChanged);
    }

    private void SetCurrentUserMemberAsDefault()
    {
        var currentMember = WorkspaceMembersState.Value.List.FirstOrDefault(
            item => item.User.Id == AuthState.Value.User.Id
        );

        model.MemberId = currentMember?.Id ?? Guid.Empty;
    }

    private void OnProjectSelected(ProjectDto? project)
    {
        model.ProjectId = project?.Id ?? Guid.Empty;
    }

    private void OnMemberSelected(WorkspaceMemberDto? member)
    {
        model.MemberId = member?.Id ?? Guid.Empty;
    }

    public void Dispose()
    {
        WorkspaceMembersState.StateChanged -= OnWorkspaceMembersStateChanged;
    }
}
