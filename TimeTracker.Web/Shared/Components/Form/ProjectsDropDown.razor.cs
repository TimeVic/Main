﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;

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
    
    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private ProjectDto? _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDown<long> _listReference;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadProjectListAction());
    }
    
    private Task OnValueChanged(long selectedId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(selectedId);
        return Task.CompletedTask;
    }
}
