using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Dialogs;

public partial class EmojiPickerModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public EventCallback OnConfirm { get; set; }
    
    [Parameter]
    public EventCallback<string> OnEmojiSelected { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }
    
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
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Cancel());
        }
        await OnClose.InvokeAsync();
    }

    private async Task SelectEmojiAsync(string emoji)
    {
        searchText = string.Empty;
        activeCategory = "All";
        await OnEmojiSelected.InvokeAsync(emoji);
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok(emoji));
        }
    }

    private string DisplayCategory(string category)
    {
        var localized = DashboardLocalizer[$"EmojiCategory_{category}"];
        return localized.ResourceNotFound ? category : localized.Value;
    }
    
    private string searchText = string.Empty;
    private string activeCategory = "All";
}
