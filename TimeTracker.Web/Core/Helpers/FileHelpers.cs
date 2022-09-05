using Microsoft.JSInterop;

namespace TimeTracker.Web.Core.Helpers;

public static class FileHelpers
{
    public static async Task SaveAsAsync(IJSRuntime js, string filename, byte[] data)
    {
        var base64String = Convert.ToBase64String(data);
        await js.InvokeAsync<object>(
            "window.saveAsFile",
            filename,
            base64String
        );
    } 
}
