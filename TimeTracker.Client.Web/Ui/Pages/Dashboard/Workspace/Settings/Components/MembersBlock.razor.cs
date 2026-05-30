using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class MembersBlock
{
    [Inject]
    private IState<WorkspaceMembersState> _state { get; set; } = default!;

    private bool _isAddMemberModalOpened { get; set; }

    private WorkspaceMemberDto? _memberToUpdate { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadListAction());
    }

    private Task OnAdd()
    {
        _isAddMemberModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnEdit(WorkspaceMemberDto member)
    {
        _memberToUpdate = member;
        return Task.CompletedTask;
    }

    private Task OnDelete(WorkspaceMemberDto member)
    {
        Dispatcher.Dispatch(new DeleteMemberAction(member));
        return Task.CompletedTask;
    }
}
