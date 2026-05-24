using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Web.Pages.Dashboard.Tag.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddTagModal()
    {
        await _mudDialogService.ShowAsync<AddTagModal>("Add new tag");
    }
    
    public async Task ShowUpdateTagModal(TagDto item)
    {
        var parameters = new DialogParameters<UpdateTagModal>
        {
            { x => x.Tag, item },
        };
        await _mudDialogService.ShowAsync<UpdateTagModal>("Update tag", parameters);
    }
}
