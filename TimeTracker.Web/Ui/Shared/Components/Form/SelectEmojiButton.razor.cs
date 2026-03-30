using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class SelectEmojiButton
{
    [Parameter]
    public EventCallback<EmojiList.EmojiOptionModel> OnSelected { get; set; }
    
    [Inject]
    public UiHelperService UiHelperService { get; set; }
    
    private async Task SelectEmoji()
    {
        await UiHelperService.OpenInNewTab(SiteUrl.Dashboard_Emoji);
    }
}
