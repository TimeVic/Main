using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TimeTracker.Web.Constants.Ui;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class SelectEmojiButton : IDisposable
{
    [Parameter]
    public EventCallback<EmojiList.EmojiOptionModel> OnSelected { get; set; }

    private IEnumerable<IGrouping<string, EmojiList.EmojiOptionModel>> GroupedEmojis => EmojiList.List
        .GroupBy(option => option.Category);

    private bool _isOpen;
    private string _panelStyle = string.Empty;
    private ElementReference _popoverElement;
    private ElementReference _triggerElement;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isOpen)
        {
            await Js.InvokeVoidAsync("popupPortal.showPopover", _popoverElement);
        }
    }

    private async Task TogglePicker()
    {
        if (_isOpen)
        {
            ClosePicker();
            return;
        }

        var panelStyle = await Js.InvokeAsync<string>(
            "popupPortal.getPanelStyle",
            [
                _triggerElement,
                340,
                420,
                8,
                12
            ]);

        _panelStyle = panelStyle;
        _isOpen = true;
    }

    private void ClosePicker()
    {
        _isOpen = false;
    }

    private async Task SelectEmoji(EmojiList.EmojiOptionModel emoji)
    {
        await OnSelected.InvokeAsync(emoji);
    }

    public void Dispose()
    {
        ClosePicker();
    }
}
