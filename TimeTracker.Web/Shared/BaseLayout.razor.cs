using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Validation;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Common.Actions;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Shared;

public partial class BaseLayout
{
    [Inject]
    protected IAuthorizationService AuthService { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IReCaptchaService ReCaptchaService { get; set; }

    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    [Inject]
    protected IState<CommonState> CommonState { get; set; }
    
    [Inject]
    protected IState<TimeEntryState> TimeEntryState { get; set; }
    
    [Inject]
    protected IDispatcher Dispatcher { get; set; }

    [Inject]
    protected IJSRuntime Js { get; set; }
    
    protected bool IsRedirectIfNotLoggedIn = true;

    protected bool IsShowMainMenu => AuthState.Value.IsLoggedIn
        && NavigationManager.GetPath().ToLower().StartsWith(SiteUrl.DashboardBase);

    protected bool IsShowReCaptcha = false;

    private bool IsSharedPage
    {
        get
        {
            var path = NavigationManager.GetPath();
            return path.Equals("/") 
                   || path.StartsWith("/login")
                   || path.StartsWith("/registration")
                   || path.StartsWith("/documentation");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        CommonState.StateChanged += async (sender, args) =>
        {
            if (CommonState.Value.IsInitialized)
            {
                await InitAppAsync();
            }
        };
        TimeEntryState.StateChanged += async (sender, args) =>
        {
            var faviconName = "black/clock-64.png";
            if (TimeEntryState.Value.HasActiveEntry)
            {
                faviconName = "play_1/play-64.png";
            }

            await Js.InvokeAsync<object>("window.setFavicon", faviconName);
        };
        Dispatcher.Dispatch(new LoadPersistedDataAction());
    }

    private async Task InitAppAsync()
    {
        NavigationManager.LocationChanged += (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
        };
        AuthState.StateChanged += async (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
            if (AuthState.Value.IsLoggedIn)
            {
                await OnAppInitializedAsync();
            }
        };
        ReCaptchaService.IsShowChanged += OnReCaptchaShowChanged;
        IsShowReCaptcha = ReCaptchaService.GetIsEnabled();
        
        await AuthService.CheckIsLoggedInAsync();
        CheckIsLoggedInAndRedirect();
        if (AuthState.Value.IsLoggedIn)
        {
            await OnAppInitializedAsync();
        }
    }

    public void Dispose()
    {
        ReCaptchaService.IsShowChanged -= OnReCaptchaShowChanged;
    }

    private void OnReCaptchaShowChanged(bool isShow)
    {
        IsShowReCaptcha = isShow;
    }
    
    private void CheckIsLoggedInAndRedirect()
    {
        if (!IsRedirectIfNotLoggedIn)
        {
            return;
        }

        if (!AuthState.Value.IsLoggedIn && !IsSharedPage)
        {
            NavigationManager.NavigateTo("/login");
        }
        StateHasChanged();
    }

    protected virtual Task OnAppInitializedAsync()
    {
        return Task.CompletedTask;
    }
}
