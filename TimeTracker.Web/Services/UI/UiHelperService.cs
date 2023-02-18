using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Services.UI;

public class UiHelperService
{
    private readonly IJSRuntime _js;

    public UiHelperService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SimulateClick(ElementReference elementReference)
    {
        await _js.InvokeAsync<object>("clickOnElement", elementReference);
    }
    
    public async Task OpenFileInNewTab(string fileName, string url)
    {
        await _js.InvokeAsync<object>("openFile", new
        {
            fileName,
            url
        });
    }
}
