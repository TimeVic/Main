using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Pages.Dashboard.Tag.Parts;

public partial class TagsList
{
    [Inject] 
    private IState<TagState> _state { get; set; }
    
    private RadzenDataGrid<TagDto> _grid;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private async Task OnDelete(TagDto item)
    {
        var isOk = await ModalDialogProviderService.ShowDeleteConfirmationDialog(
            "Are you sure you want to remove this tag?"
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
        await ModalDialogProviderService.ShowAddTagModal();
    }

    private async Task OnEdit(TagDto item)
    {
        await ModalDialogProviderService.ShowUpdateTagModal(item);
    }
}
