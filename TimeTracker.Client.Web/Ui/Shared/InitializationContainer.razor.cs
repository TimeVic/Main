using Fluxor;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Web.Services.Notification;
using TimeTracker.Client.Web.Services.Validation;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.TimeEntry;
using ActiveTimeEntrySynchronizationTimer = System.Timers.Timer;

namespace TimeTracker.Client.Web.Ui.Shared;

public partial class InitializationContainer : IDisposable
{
    private const double ActiveTimeEntrySynchronizationInterval = 30_000;
    private ActiveTimeEntrySynchronizationTimer? _activeTimeEntrySynchronizationTimer;
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsRegisterPushNotificationToken { get; set; }

    [Inject]
    protected IJSRuntime Js { get; set; } = default!;
    
    [Inject]
    protected IReCaptchaService ReCaptchaService { get; set; } = default!;
    
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; } = default!;
    
    [Inject]
    protected IState<TimeEntryState> TimeEntryState { get; set; } = default!;

    [Inject]
    protected IDispatcher TimeEntryDispatcher { get; set; } = default!;
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;
    
    [Inject]
    protected WorkspaceInitializationService WorkspaceInitializationService { get; set; } = default!;

    [Inject]
    protected FcmService FcmService { get; set; } = null!;

    [Inject]
    protected ILogger<InitializationContainer> Logger { get; set; } = null!;

    [Inject]
    protected UrlService UrlService { get; set; } = default!;
    
    [Inject]
    protected IState<CommonState> CommonState { get; set; } = default!;
    
    protected override async Task OnInitializedAsync()
    {
        TimeEntryState.StateChanged += OnTimeEntryStateChanged;
        if (!NavigationManager.GetPath().StartsWith("/board-change/"))
        {
            var workspaceId = UrlService.GetWorkspaceIdFromDashboardUrl();
            if (!await WorkspaceInitializationService.EnsureWorkspaceAsync(workspaceId))
            {
                NavigationManager.NavigateTo(SiteUrl.Error403, replace: true);
                return;
            }

            WorkspaceInitializationService.Init();
            await WorkspaceInitializationService.AfterInit();

            if (IsRegisterPushNotificationToken)
            {
                try
                {
                    await FcmService.SetNotificationToken();
                }
                catch (Exception exception)
                {
                    Logger.LogWarning(exception, "Unable to register the browser push notification token");
                }
            }

            StartActiveTimeEntrySynchronization();

            if (!workspaceId.HasValue && AuthState.Value.Workspace != null)
            {
                NavigationManager.NavigateTo(
                    UrlService.GetDashboardUrlForCurrentPath(AuthState.Value.Workspace.Id),
                    replace: true
                );
            }
        }
    }

    private void StartActiveTimeEntrySynchronization()
    {
        _activeTimeEntrySynchronizationTimer ??= new ActiveTimeEntrySynchronizationTimer(ActiveTimeEntrySynchronizationInterval);
        _activeTimeEntrySynchronizationTimer.Elapsed += OnActiveTimeEntrySynchronizationElapsed;
        _activeTimeEntrySynchronizationTimer.Start();
    }

    private void OnActiveTimeEntrySynchronizationElapsed(object? sender, ElapsedEventArgs e)
    {
        _ = InvokeAsync(() => TimeEntryDispatcher.Dispatch(new LoadActiveTimeEntryAction()));
    }

    private async void OnTimeEntryStateChanged(object? sender, EventArgs args)
    {
        var faviconName = TimeEntryState.Value.HasActiveEntry
            ? "/assets/brand/timevic-app-icon.svg"
            : "/favicon.svg";
        await Js.InvokeAsync<object>("window.setFavicon", faviconName);
    }

    public void Dispose()
    {
        TimeEntryState.StateChanged -= OnTimeEntryStateChanged;
        if (_activeTimeEntrySynchronizationTimer == null)
        {
            return;
        }

        _activeTimeEntrySynchronizationTimer.Elapsed -= OnActiveTimeEntrySynchronizationElapsed;
        _activeTimeEntrySynchronizationTimer.Dispose();
    }
}
