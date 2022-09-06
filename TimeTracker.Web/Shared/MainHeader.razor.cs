﻿using System.Globalization;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Shared;

public partial class MainHeader
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }
    
    [Inject]
    private IState<AuthState> AuthState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private async Task OnSelectLanguageAsync(string id)
    {
        NavigationManager.NavigateTo("/", forceLoad: true);
    }
    
    private void OnLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        NavigationManager.NavigateTo("/", true);
    }
    
    private void OnClickUserMenu(RadzenProfileMenuItem menuEvent)
    {
        switch (menuEvent.Value)
        {
            case "logout":
                Dispatcher.Dispatch(new LogoutAction());
                NavigationManager.NavigateTo("/", true);
                break;
        }
    }
}
