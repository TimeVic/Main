using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TimeTracker.Client.Core.Services.UI;

public sealed record TextAreaState(string Value, int SelectionStart, int SelectionEnd);

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
    
    public async Task OpenInNewTab(string url)
    {
        await _js.InvokeVoidAsync("openInNewTab", url);
    }
    
    public async Task<bool> CopyToClipboard(string text)
    {
        return await _js.InvokeAsync<bool>("copyToClipboard", text);
    }
    
    public async Task<bool> IsTextSelected()
    {
        return await _js.InvokeAsync<bool>("isTextSelected");
    }

    public async Task<TextAreaState> GetTextAreaState(ElementReference editor)
    {
        return await _js.InvokeAsync<TextAreaState>("getTextAreaState", editor);
    }

    public async Task SetTextAreaSelection(ElementReference editor, int selectionStart, int selectionEnd)
    {
        await _js.InvokeVoidAsync("setTextAreaSelection", editor, selectionStart, selectionEnd);
    }
    
    public async Task ScrollToBottom(string elementId)
    {
        await _js.InvokeVoidAsync("scrollHelper.scrollToBottom", elementId);
    }
    
    public async Task<bool> HasScroll(string elementId)
    {
        return await _js.InvokeAsync<bool>("scrollHelper.hasScroll", elementId);
    }
    
    /**
     * Usage:
     * 
         [JSInvokable]
         public Task OnScrollTopReached()
         {
           Console.WriteLine("User scrolled to TOP!");
           return Task.CompletedTask;
         }
     */
    public async Task OnScrollTopReached<TValue>(string elementId, DotNetObjectReference<TValue> component) where TValue : class
    {
        await _js.InvokeVoidAsync("scrollHelper.onScrollTopReached", elementId, component);
    }
}
