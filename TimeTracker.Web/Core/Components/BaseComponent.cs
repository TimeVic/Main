﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Core.Components;

public class BaseComponent: Fluxor.Blazor.Web.Components.FluxorComponent
{
    [Parameter]
    public string? Locale { get; set; }
    
    [Inject]
    protected IDispatcher Dispatcher { get; set; }
    
    [Inject]
    protected IJSRuntime Js { get; set; }
    
    [Inject]
    protected ApiService ApiService { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    #region Radzen
    
    [Inject] 
    protected NotificationService NotificationService { get; set; }
    
    [Inject]
    protected DialogService DialogService { get; set; }
    
    [Inject]
    protected ModalDialogProviderService ModalDialogProviderService { get; set; }
    
    #endregion
}
