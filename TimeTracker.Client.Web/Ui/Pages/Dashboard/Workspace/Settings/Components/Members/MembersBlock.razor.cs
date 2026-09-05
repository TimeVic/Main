using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members;

public partial class MembersBlock
{
    [Inject]
    private IState<WorkspaceMembersState> _state { get; set; } = default!;

    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadListAction());
    }

    private async Task OnAdd()
    {
        await _modalDialogService.ShowAddWorkspaceMemberModal();
    }

    private async Task OnEdit(WorkspaceMemberDto member)
    {
        await _modalDialogService.ShowUpdateWorkspaceMemberModal(member);
    }

    private Task OnDelete(WorkspaceMemberDto member)
    {
        Dispatcher.Dispatch(new DeleteMemberAction(member));
        return Task.CompletedTask;
    }
}
