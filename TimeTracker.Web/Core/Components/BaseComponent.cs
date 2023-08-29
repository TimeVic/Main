using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
    protected ToastService ToastService { get; set; }
    
    [Inject]
    protected ModalDialogProviderService ModalDialogService { get; set; }
    
    #endregion
    
    private List<Action> _actionsToRunAfterRender = new List<Action>();
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        // run all the actions (.NET code) **once** after rendering
        foreach (var actionToRun in _actionsToRunAfterRender)
        {
            actionToRun();
        }
        // clear the actions to make sure the actions only run **once**
        _actionsToRunAfterRender.Clear();
        return base.OnAfterRenderAsync(firstRender);
    }
    
    /// <summary>
    /// Run an action once after the component is rendered
    /// </summary>
    /// <param name="action">Action to invoke after render</param>
    protected void RunAfterRendered(Action action) => _actionsToRunAfterRender.Add(action);
}
