using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

public partial class AddMemberModal
{
    private sealed class InviteModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please enter an email address or login")]
        public string EmailOrLogin { get; set; } = string.Empty;

        public MembershipAccessType Access { get; set; } = MembershipAccessType.User;
    }

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    private readonly ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>
    {
        MembershipAccessType.User,
        MembershipAccessType.Manager
    };

    private InviteModel _model = new();
    private EditForm _form = default!;
    private bool _isLoading;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        _isLoading = true;
        try
        {
            var workspaceId = UrlService.GetWorkspaceIdFromDashboardUrl()
                ?? AuthState.Value.Workspace?.Id;
            if (workspaceId.HasValue)
            {
                var member = await ApiService.WorkspaceMemberAddAsync(workspaceId.Value, _model.EmailOrLogin, _model.Access);
                if (member != null)
                {
                    Dispatcher.Dispatch(new LoadListAction(true));
                    ToastService.ShowInfo(DashboardLocalizer["AddMemberModal_InvitationSent"].Value);
                    OnCloseModal();
                }
            }
        }
        catch (Exception e)
        {
            ToastService.ShowError(string.IsNullOrWhiteSpace(e.Message)
                ? DashboardLocalizer["AddMemberModal_InvitationError"].Value
                : e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private void OnCloseModal()
    {
        _model = new InviteModel();
        ModalInstance?.Close(AppModalResult.Ok("submit"));
    }
}
