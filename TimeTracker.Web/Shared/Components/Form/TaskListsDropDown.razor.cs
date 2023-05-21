using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TaskListsDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;
    
    [Parameter]
    public long Value
    {
        get => _selectedId;
        set => _selectedId = value;
    }
    
    [Parameter]
    public EventCallback<long> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TaskListDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select task list";
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long ProjectId { get; set; }

    [Inject]
    public ILogger<TaskListDto> _logger { get; set; }
    
    [Inject]
    public ApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<TasksListState> _state { get; set; }
    
    private TaskListDto? _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDown<long> _listReference;

    private ICollection<TaskListDto> _list
    {
        get
        {
            var list = _state.Value.List;
            if (ProjectId == 0)
            {
                return list;
            }

            return list.Where(item => item.Project?.Id == ProjectId).ToList();
        }
    }

    private Task OnValueChanged(long selectedId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(selectedId);
        return Task.CompletedTask;
    }
}
