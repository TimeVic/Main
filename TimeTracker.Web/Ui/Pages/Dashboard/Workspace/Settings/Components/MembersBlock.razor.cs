using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class MembersBlock
{
    [Inject]
    private IState<WorkspaceMembersState> _state { get; set; } = default!;

    private WorkspaceMemberDto? _memberToUpdate { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadListAction());
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

