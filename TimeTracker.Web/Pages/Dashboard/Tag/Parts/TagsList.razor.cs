using Fluxor;
using Microsoft.AspNetCore.Components;
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

    private async Task OnDeleteItemAsync(TagDto value)
    {
        await Task.CompletedTask;
    }
    
    private async Task InsertRow()
    {
        Dispatcher.Dispatch(new AddEmptyListItemAction());
        // await _grid.GoToPage(0);
        await EditRow(_state.Value.ItemToAdd);
    }
    
    private async Task EditRow(TagDto item)
    {
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(TagDto item)
    {
        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(TagDto item)
    {
        Dispatcher.Dispatch(new RemoveEmptyListItemAction());
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(TagDto item)
    {
        if (item.Id > 0)
        {
            Dispatcher.Dispatch(new UpdateItemAction(item));
            return;
        }

        Dispatcher.Dispatch(new SaveEmptyListItemAction());
    }
    
    #region ColorPicker helpers

    private string? GetColorRgb(string? colorHex)
    {
        if (string.IsNullOrEmpty(colorHex))
            return "";
        return RGB.Parse(colorHex).ToCSS();
    }
    
    private string? GetColorHex(string? colorRgb)
    {
        if (string.IsNullOrEmpty(colorRgb))
            return "";
        return "#" + RGB.Parse(colorRgb).ToHex();
    }

    #endregion
}
