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
    protected IDispatcher Dispatcher { get; set; }
    
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
                InitAppAsync();
                IsShowReCaptcha = ReCaptchaService.GetIsEnabled();
                StateHasChanged();
            }
        };
        ReCaptchaService.IsShowChanged += OnReCaptchaShowChanged;
        Dispatcher.Dispatch(new LoadPersistedDataAction());
    }

    public void Dispose()
    {
        ReCaptchaService.IsShowChanged -= OnReCaptchaShowChanged;
    }
    
    private void OnReCaptchaShowChanged(bool isShow)
    {
        IsShowReCaptcha = isShow;
    }
    
    private void InitAppAsync()
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
                await OnLoggedInAsync();
            }
        };
        CommonState.StateChanged += async (sender, args) =>
        {
            if (CommonState.Value.IsWorkspaceInitialized)
            {
                // await AuthService.CheckIsLoggedInAsync();
                CheckIsLoggedInAndRedirect();    
            }
        };
        
        
        if (AuthState.Value.IsLoggedIn)
        {
            OnLoggedInAsync();
        }
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
    
    protected virtual Task OnLoggedInAsync()
    {
        return Task.CompletedTask;
    }
}
