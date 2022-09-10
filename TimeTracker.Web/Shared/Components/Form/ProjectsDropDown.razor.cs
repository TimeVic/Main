using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ProjectsDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public long Value
    {
        get => _selectedId;
        set => _selectedId = value;
    }
    
    [Parameter]
    public EventCallback<long> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<ProjectDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select project";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<ProjectDto> _logger { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private ICollection<ProjectDto> _list = new List<ProjectDto>();

    private ICollection<ProjectDto> _listToShow = new List<ProjectDto>();

    private int _count = 0;
    
    private int _page = 0;
    
    private bool _hasMore = true;
    
    private int _skip = 0;
    
    private int _take = 0;

    private ProjectDto _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDownDataGrid<long> _listReference;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNextList();
    }

    private async Task LoadData(LoadDataArgs filter)
    {
        await LoadNextList();
        var query = _list.AsQueryable();
        if (filter.Skip.HasValue)
        {
            query = query.Skip(filter.Skip.Value);
        }
        if (filter.Top.HasValue)
        {
            query = query.Take(filter.Top.Value);
        }
        _listToShow = query.ToList();
    }

    private async Task LoadNextList()
    {
        if (!_hasMore)
        {
            return;
        }

        _page += 1;
        var response = await _apiService.ProjectGetListAsync(new GetListRequest()
        {
            WorkspaceId = _authState.Value.Workspace.Id,
            Page = _page
        });
        if (response == null)
        {
            return;
        }

        _hasMore = response.IsHasMore;
        _count = response.TotalCount;
        lock (_list)
        {
            _list = _list.Concat(response.Items).ToList();
        }
        var selectedItem = _list.FirstOrDefault(item => item.Id == _selectedId);
        long.TryParse(_listReference.SelectedValue?.ToString(), out var selectedValue);
        if (selectedItem != null && selectedValue != _selectedId)
        {
            await _listReference.SelectItem(selectedItem);    
        }
    }
    
    private Task OnValueChanged(long selectedId)
    {
        _selectedItem = _list.FirstOrDefault(item => item.Id == selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(selectedId);
        return Task.CompletedTask;
    }
}
