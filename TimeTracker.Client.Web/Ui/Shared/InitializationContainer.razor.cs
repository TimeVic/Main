using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Web.Services.Validation;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Shared;

public partial class InitializationContainer
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    protected IJSRuntime Js { get; set; }
    
    [Inject]
    protected IReCaptchaService ReCaptchaService { get; set; }
    
    [Inject]
    protected IAuthorizationService AuthService { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    [Inject]
    protected IState<TimeEntryState> TimeEntryState { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    [Inject]
    protected WorkspaceInitializationService WorkspaceInitializationService { get; set; }
    
    [Inject]
    protected IState<CommonState> CommonState { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        TimeEntryState.StateChanged += async (sender, args) =>
        {
            var faviconName = "/favicon.svg";
            if (TimeEntryState.Value.HasActiveEntry)
            {
                faviconName = "/assets/brand/timevic-app-icon.svg";
            }

            await Js.InvokeAsync<object>("window.setFavicon", faviconName);
        };
        if (!NavigationManager.GetPath().StartsWith("/board-change/"))
        {
            WorkspaceInitializationService.Init();
            await WorkspaceInitializationService.AfterInit();
        }
    }
}
