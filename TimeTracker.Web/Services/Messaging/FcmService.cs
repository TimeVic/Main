using Microsoft.JSInterop;
using MudBlazor;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Services.Messaging;

public class FcmService
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _jsFcmModule;

    public FcmService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task Test()
    {
        // _jsFcmModule = await _js.InvokeAsync<IJSObjectReference>("import", "./js/messaging-init.js");
        // var result = await _jsFcmModule.InvokeAsync<string>("getGcmToken");
        // Debug.Log(result);
    }
}
