using LumexUI;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Services.UI.Modal;

namespace TimeTracker.Web.Ui.Shared.Components.Dialogs;

public partial class EmojiPickerModal: IModalComponent
{
    [Parameter]
    public EventCallback OnConfirm { get; set; }
    
    [Parameter]
    public EventCallback<string> OnEmojiSelected { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
    [Parameter]
    public ModalInstance ModalInstance { get; set; }
    
    private IEnumerable<string> Categories => new[] { "All" }
        .Concat(EmojiList.List.Select(option => option.Category).Distinct(StringComparer.Ordinal).OrderBy(category => category, StringComparer.Ordinal));

    private IEnumerable<EmojiList.EmojiOptionModel> FilteredEmojis => EmojiList.List.Where(option => MatchesCategory(option) && MatchesSearch(option));
    
    private void SetCategory(string category)
    {
        activeCategory = category;
    }

    private bool MatchesCategory(EmojiList.EmojiOptionModel option)
    {
        return activeCategory == "All" || string.Equals(option.Category, activeCategory, StringComparison.Ordinal);
    }

    private bool MatchesSearch(EmojiList.EmojiOptionModel option)
    {
        var query = searchText.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var haystack = string.Join(" ", new[] { option.Symbol, option.Name, option.HtmlCode, option.Category, option.Keywords });
        return haystack.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private async Task CloseAsync()
    {
        searchText = string.Empty;
        activeCategory = "All";
        await modal.CloseAsync();
        await OnClose.InvokeAsync();
    }

    private async Task SelectEmojiAsync(string emoji)
    {
        await CloseAsync();
        await OnEmojiSelected.InvokeAsync(emoji);
    }
    
    private string searchText = string.Empty;
    private string activeCategory = "All";
    LumexModal modal;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
}
