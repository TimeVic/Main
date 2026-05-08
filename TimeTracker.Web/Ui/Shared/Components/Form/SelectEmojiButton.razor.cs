using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants.Ui;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class SelectEmojiButton
{
    [Parameter]
    public EventCallback<EmojiList.EmojiOptionModel> OnSelected { get; set; }

    private IEnumerable<string> Categories => new[] { "All" }
        .Concat(EmojiList.List
            .Select(option => option.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(category => category, StringComparer.Ordinal));

    private IEnumerable<EmojiList.EmojiOptionModel> FilteredEmojis => EmojiList.List
        .Where(option => MatchesCategory(option) && MatchesSearch(option));

    private bool _isOpen;
    private string _searchText = string.Empty;
    private string _activeCategory = "All";

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private void SetCategory(string category)
    {
        _activeCategory = category;
    }

    private bool MatchesCategory(EmojiList.EmojiOptionModel option)
    {
        return _activeCategory == "All" || string.Equals(option.Category, _activeCategory, StringComparison.Ordinal);
    }

    private bool MatchesSearch(EmojiList.EmojiOptionModel option)
    {
        var query = _searchText.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var haystack = string.Join(" ", option.Symbol, option.Name, option.HtmlCode, option.Category, option.Keywords);
        return haystack.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SelectEmoji(EmojiList.EmojiOptionModel emoji)
    {
        _isOpen = false;
        _searchText = string.Empty;
        _activeCategory = "All";
        await OnSelected.InvokeAsync(emoji);
    }

    private string GetCategoryClass(string category)
    {
        return _activeCategory == category
            ? "rounded-full bg-blue-600 px-2.5 py-1 text-xs font-semibold text-white"
            : "rounded-full border border-slate-200 bg-white px-2.5 py-1 text-xs font-semibold text-slate-500 transition hover:border-blue-200 hover:bg-blue-50 hover:text-blue-700";
    }
}
