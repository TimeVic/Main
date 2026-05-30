using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Web.Pages.Dashboard.Members.Parts;
using TimeTracker.Client.Web.Pages.Dashboard.Members.Parts.List;
using TimeTracker.Client.Web.Pages.Dashboard.Tag.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddWorkspaceMemberModal()
    {
        await _mudDialogService.ShowAsync<AddMemberModal>("Add new member");
    }
    
    public async Task ShowUpdateWorkspaceMemberModal(WorkspaceMemberDto item)
    {
        var parameters = new DialogParameters<MemberAccessModal>
        {
            { x => x.WorkspaceMember, item },
        };
        await _mudDialogService.ShowAsync<MemberAccessModal>(
            "Update member",
            parameters
        );
    }
}
