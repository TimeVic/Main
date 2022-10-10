using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts.List;

public partial class AddMemberForm
{
    [Inject]
    public IState<AuthState> AuthState { get; set; }

    private AddRequest model = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.WorkspaceId = AuthState.Value.Workspace.Id;
    }

    private async Task HandleSubmit()
    {
        Dispatcher.Dispatch(new AddNewMemberAction(model.Email));
        DialogService.Close();
    }
}
