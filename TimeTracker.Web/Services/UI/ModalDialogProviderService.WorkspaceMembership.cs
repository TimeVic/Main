using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Members.Parts;
using TimeTracker.Web.Pages.Dashboard.Members.Parts.List;
using TimeTracker.Web.Pages.Dashboard.Tag.Parts;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddWorkspaceMembershipModal()
    {
        await _mudDialogService.ShowAsync<AddMemberModal>("Add new member");
    }
    
    public async Task ShowUpdateWorkspaceMembershipModal(WorkspaceMembershipDto item)
    {
        var parameters = new DialogParameters<MemberAccessModal>
        {
            { x => x.WorkspaceMembership, item },
        };
        await _mudDialogService.ShowAsync<MemberAccessModal>(
            "Update member",
            parameters
        );
    }
}
