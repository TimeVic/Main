using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tag.Parts;

public partial class TagsList
{
    [Inject] 
    private IState<TagState> _state { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private async Task OnDelete(TagDto item)
    {
        var isOk = await ModalDialogService.ShowDeleteConfirmationDialog(
            DashboardLocalizer["TagsList_RemoveTagConfirmation"].Value
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new DeleteItemAction(item));
        }
    }

    public MudColor? GetMudColor(string? color)
    {
        if (string.IsNullOrEmpty(color))
            return null;
        return new MudColor(color);
    }

    private async Task OnAdd()
    {
        await ModalDialogService.ShowAddTagModal();
    }

    private async Task OnEdit(TagDto item)
    {
        await ModalDialogService.ShowUpdateTagModal(item);
    }
}
