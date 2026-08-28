using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

public partial class AddMemberModal
{
    private sealed class InviteModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

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
                var member = await ApiService.WorkspaceMemberAddAsync(workspaceId.Value, _model.Email);
                if (member != null)
                {
                    Dispatcher.Dispatch(new LoadListAction(true));
                    ToastService.ShowInfo(DashboardLocalizer["AddMemberModal_InvitationSent"].Value);
                    await OnCloseModal();
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

    private async Task OnCloseModal()
    {
        _model = new InviteModel();
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
