using Fluxor;
using LumexUI;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class TagsDropDown
{
    [Parameter] 
    public string? Label { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<Guid> Value
    {
        get => _selectedIds;
        set => _selectedIds = value?.ToList() ?? [];
    }
    
    [Parameter]
    public EventCallback<IEnumerable<Guid>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<TagDto>> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = string.Empty;
    
    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool FullWidth { get; set; } = true;

    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Outlined;

    [Inject]
    public IState<TagState> _state { get; set; }

    private ICollection<Guid> _selectedIds = [];
    
    private static readonly SelectSlots _selectClasses = new()
    {
        Trigger = "h-auto min-h-10 min-w-0 max-w-full py-2 items-start overflow-hidden",
        InnerWrapper = "h-auto min-h-6 min-w-0 max-w-full flex-wrap items-start gap-1.5 overflow-hidden",
        Value = "flex min-w-0 max-w-full flex-wrap items-center gap-1.5 overflow-hidden whitespace-normal text-clip"
    };
    
    private ICollection<TagDto> _selectedItems => _list
        .Where(item => _selectedIds.Contains(item.Id))
        .ToList();

    private ICollection<TagDto> _list = [];

    private string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectTags"].Value : Placeholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(ICollection<TagDto> selectedTags)
    {
        _selectedIds = selectedTags.Select(item => item.Id).ToList();
        InvokeAsync(async () =>
        {
            await SelectedItemChanged.InvokeAsync(_selectedItems);
            await ValueChanged.InvokeAsync(_selectedIds.ToList());
        });
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        StateHasChanged();
    }

    private static string GetColorStyle(TagDto? tag)
    {
        return $"background-color: {tag?.Color ?? "#CBD5E1"};";
    }
}
