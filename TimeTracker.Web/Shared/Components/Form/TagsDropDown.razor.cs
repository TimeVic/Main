using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TagsDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public ICollection<long> Value
    {
        get => _selectedIds;
        set => _selectedIds = value;
    }
    
    [Parameter]
    public EventCallback<ICollection<long>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<ICollection<TagDto>> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select tags";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<TagsDropDown> _logger { get; set; }

    [Inject]
    public IState<TagState> _state { get; set; }
    
    private ICollection<TagDto> _selectedItems = new List<TagDto>();

    private ICollection<long> _selectedIds = new List<long>();
    
    private RadzenDropDown<IEnumerable<long>> _listReference;

    private void OnValueChanged(IEnumerable<long>? selectedIds)
    {
        selectedIds ??= new List<long>();
        _selectedItems = _state.Value.List.Where(
            item => selectedIds.Any(selectedId => selectedId == item.Id)
        ).ToList();
        InvokeAsync(async () =>
        {
            await SelectedItemChanged.InvokeAsync(_selectedItems);
            await ValueChanged.InvokeAsync(selectedIds.ToList());
        });
    }
}
