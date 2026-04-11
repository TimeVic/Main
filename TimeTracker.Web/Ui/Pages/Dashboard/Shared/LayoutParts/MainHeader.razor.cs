using System.Timers;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainHeader: IDisposable
{
    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> WorkspaceState { get; set; }
    
    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    [Inject]
    public UserDateTimeProviderService _dateTimeProviderService { get; set; }
    
    private bool _isShowAddWorkspaceModal = false;
    private System.Timers.Timer _timer;
    private DateTimeOffset _currentTime;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _timer = new System.Timers.Timer();
        _timer.Interval = 1000;
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        _currentTime = _dateTimeProviderService.GetCurrentTime();
        StateHasChanged();
    }

    private void OnSelectWorkspace(WorkspaceDto? workspace)
    {
        if (workspace == null)
        {
            // Clicked on selected item
            return;
        }
        _workspaceInitialization.ChangeWorkspace(workspace);
    }

    private void ToggleProfileMenu()
    {
        
    }

    private Task OnClickLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Stop();
    }
}
