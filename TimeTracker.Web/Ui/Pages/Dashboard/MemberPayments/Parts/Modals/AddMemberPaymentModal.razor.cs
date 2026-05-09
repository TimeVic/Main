using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.MemberPayments;
using TimeTracker.Web.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

public partial class AddMemberPaymentModal : IDisposable
{   
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Inject]
    public ILogger<AddMemberPaymentModal> _logger { get; set; }

    [Inject]
    public ISecurityManager SecurityManager { get; set; }

    [Inject]
    public IState<WorkspaceMembersState> WorkspaceMembersState { get; set; }
    
    private AddRequest model;
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;
    private LumexModal modal;
    private bool CanCreatePaymentForOtherMembers =>
        SecurityManager.HasPermission(WorkspacePermission.CreateMemberPaymentForOtherMembers);

    protected override async Task OnInitializedAsync()
    {
        InitModel();
        WorkspaceMembersState.StateChanged += OnWorkspaceMembersStateChanged;
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        Dispatcher.Dispatch(new AddMemberPaymentAction(model));
        InitModel();
        await modal.CloseAsync();
        StateHasChanged();
    }

    private void OnCloseModal()
    {
        IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
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
        // Fixes the add payment form default member so it uses the current user's workspace member.
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
