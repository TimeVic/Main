using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ProjectsDropDown
{
    [Parameter]
    public Expression<Func<long>>? For { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public bool Clearable { get; set; } = true;
    
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

    [Parameter]
    public long ClientId { get; set; }

    [Inject]
    public ILogger<ProjectDto> _logger { get; set; }
    
    [Inject]
    public ApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private ProjectDto? _selectedItem;

    private long _selectedId = 0;
    
    private ICollection<ProjectDto> _list
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

    private Task OnValueChanged(long projectId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == projectId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(_selectedItem?.Id ?? 0);
        return Task.CompletedTask;
    }
    
    private string ToStringFunc(long projectId)
    {
        var item = _state.Value.List.FirstOrDefault(item => item.Id == projectId);
        return item?.Name ?? string.Empty;
    }
}
