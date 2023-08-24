using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TagsDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<long> Value
    {
        get => _selectedIds;
        set => _selectedIds = value;
    }
    
    [Parameter]
    public EventCallback<IEnumerable<long>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<TagDto>> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select tags";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<TagsDropDown> _logger { get; set; }

    [Inject]
    public IState<TagState> _state { get; set; }

    private IEnumerable<TagDto> _selectedItems
    {
        get
        {
            return _state.Value.List.Where(
                item => _selectedIds.Any(selectedId => selectedId == item.Id)
            ).ToList();
        }
    }

    private IEnumerable<long> _selectedIds = new List<long>();
    private MudSelect<long> _select;

    private void OnValueChanged(IEnumerable<long>? selectedIds)
    {
        _selectedIds = selectedIds ?? new List<long>();
        InvokeAsync(async () =>
        {
            await SelectedItemChanged.InvokeAsync(_selectedItems);
            await ValueChanged.InvokeAsync(_selectedIds.ToList());
        });
    }

    private string ToStringFunc(long tagId)
    {
        var item = _state.Value.List.FirstOrDefault(item => item.Id == tagId);
        return item?.Name ?? string.Empty;
    }
}
