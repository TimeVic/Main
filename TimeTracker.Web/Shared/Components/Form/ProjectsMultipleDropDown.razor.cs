using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ProjectsMultipleDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<long> Value
    {
        get => _selectedIds;
        set
        {
            if (
                _selectedIds.Count() != value.Count()
                || !_selectedIds.All(value.Contains)
            )
            {
                _selectedIds = value;
            }
        }
    }
    
    [Parameter]
    public EventCallback<IEnumerable<long>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<ProjectDto>> SelectedItemsChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select project";
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long ClientId { get; set; }

    [Inject]
    public ILogger<ProjectDto> _logger { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private IEnumerable<ProjectDto> _selectedItems;

    private IEnumerable<long> _selectedIds = new List<long>();
    
    private RadzenDropDown<IEnumerable<long>> _listReference;

    private IEnumerable<ProjectDto> _list
    {
        get
        {
            var list = _state.Value.List;
            if (ClientId == 0)
            {
                return list;
            }

            return list.Where(item => item.Client?.Id == ClientId).ToList();
        }
    }

    private Task OnValueChanged(IEnumerable<long>? selectedIds)
    {
        selectedIds ??= new List<long>();
        _selectedItems = _state.Value.List.Where(item => selectedIds.Contains(item.Id)).ToList();
        SelectedItemsChanged.InvokeAsync(_selectedItems);
        ValueChanged.InvokeAsync(selectedIds);
        return Task.CompletedTask;
    }
}
