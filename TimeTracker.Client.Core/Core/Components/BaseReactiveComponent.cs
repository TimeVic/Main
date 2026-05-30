using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Client.Core.Services.DateTimes;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Core.Components;

public class BaseReactiveComponent: Fluxor.Blazor.Web.Components.FluxorComponent
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
    
    [Inject] 
    protected IToastService ToastService { get; set; }
    
    [Inject] 
    protected UserDateTimeProviderService UserDateTimeProviderService { get; set; }
    
    private List<Action> _actionsToRunAfterRender = [];
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
