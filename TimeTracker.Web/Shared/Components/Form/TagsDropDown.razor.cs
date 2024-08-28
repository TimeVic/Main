using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TagsDropDown
{
    [Parameter] 
    public string Label { get; set; }
    
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

    private IEnumerable<long> _selectedIds = new List<long>();
    
    private ICollection<TagDto> _selectedItems => _list.Where(item => _selectedIds.Contains(item.Id)).ToList();
    private ICollection<TagDto> _list = new List<TagDto>();
    private long _selectedId = 0;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(IEnumerable<TagDto> selectedTags)
    {
        _selectedIds = selectedTags.Select(item => item.Id);
        InvokeAsync(async () =>
        {
            await SelectedItemChanged.InvokeAsync(_selectedItems);
            await ValueChanged.InvokeAsync(_selectedIds.ToList());
        });
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List;
    }
}
