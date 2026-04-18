using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class MembershipsBlock
{
    [Inject]
    private IState<WorkspaceMembershipsState> _state { get; set; } = default!;

    private WorkspaceMembershipDto? _memberToUpdate { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadListAction());
    }

    private Task OnEdit(WorkspaceMembershipDto membership)
    {
        _memberToUpdate = membership;
        return Task.CompletedTask;
    }

    private Task OnDelete(WorkspaceMembershipDto membership)
    {
        Dispatcher.Dispatch(new DeleteMemberAction(membership));
        return Task.CompletedTask;
    }
}

