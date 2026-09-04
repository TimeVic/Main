using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Core.Components;
using TimeTracker.Client.Core.Store.Tag;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class TagsSelect : BaseReactiveComponent, IDisposable
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public SelectSize Size { get; set; } = SelectSize.Medium;

    [Parameter]
    public SelectVariant Variant { get; set; } = SelectVariant.Input;

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

    [Inject]
    public IState<TagState> _state { get; set; }

    private ICollection<Guid> _selectedIds = [];
    private ICollection<TagDto> _list = [];

    private ICollection<TagDto> _selectedTagList => _list
        .Where(item => _selectedIds.Contains(item.Id))
        .ToList();

    protected string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectTags"].Value : Placeholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _state.StateChanged += OnTagStateChanged;
        UpdateList();
    }

    public void Dispose()
    {
        _state.StateChanged -= OnTagStateChanged;
    }

    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        InvokeAsync(StateHasChanged);
    }

    private void OnTagStateChanged(object? sender, EventArgs args)
    {
        UpdateList();
    }

    private async Task OnTagsChanged(ICollection<TagDto> selectedTags)
    {
        _selectedIds = selectedTags.Select(item => item.Id).ToList();
        await SelectedItemChanged.InvokeAsync(selectedTags);
        await ValueChanged.InvokeAsync(_selectedIds);
    }

    private static string GetColorStyle(TagDto? tag)
    {
        return $"background-color: {tag?.Color ?? "#CBD5E1"};";
    }
}
