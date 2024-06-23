using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
    public Expression<Func<long>>? For { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }
    
    [Parameter] 
    public bool Required { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;
    
    [Parameter]
    public long Value
    {
        get => _selectedId;
        set => _selectedId = value;
    }
    
    [Parameter]
    public EventCallback<long?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TaskListDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select task list";
    
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long? ProjectId { get; set; }
    
    [Parameter]
    public long? ClientId { get; set; }

    [Parameter]
    public bool IsExtendedInfo { get; set; } = true;

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
    
    private ICollection<TaskListDto> _list
    {
        get
        {
            var list = _state.Value.List;
            if (ProjectId.HasValue)
            {
                return list.Where(item => item.Project?.Id == ProjectId).ToList();
            }
            else if (ClientId.HasValue)
            {
                return list.Where(item => item.Project?.Client?.Id == ClientId).ToList();
            }
            return list;
        }
    }

    private Task OnValueChanged(long taskListId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == taskListId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(_selectedItem?.Id ?? 0);
        return Task.CompletedTask;
    }

    private string ToStringFunc(long taskListId)
    {
        var item = _state.Value.List.FirstOrDefault(item => item.Id == taskListId);
        return item?.Name ?? string.Empty;
    }
}
