using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Shared.Components.List;

public partial class TPaginator
{
    private readonly record struct PageItem(int Page, bool IsEllipsis);

    [Parameter] public int CurrentPage { get; set; } = 1;

    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }
    
    [Parameter]
    public int TotalPages { get; set; } = 1;

    [Parameter]
    public int? TotalItems { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string ItemsLabel { get; set; } = string.Empty;
    
    [Parameter]
    public string SummaryText { get; set; } = string.Empty;
    
    [Parameter]
    public int MaxVisiblePages { get; set; } = 5;

    private string? _loadingControlKey;
    private bool _wasLoading;

    private bool IsVisible => TotalItems.GetValueOrDefault() > 0 || TotalPages > 0;

    private bool IsOnFirstPage => SafeCurrentPage <= 1;

    private bool IsOnLastPage => SafeCurrentPage >= SafeTotalPages;

    private int SafeTotalPages => Math.Max(1, TotalPages);

    private int SafeCurrentPage => Math.Min(Math.Max(1, CurrentPage), SafeTotalPages);

    protected override void OnParametersSet()
    {
        if (_wasLoading && !IsLoading)
        {
            _loadingControlKey = null;
        }

        _wasLoading = IsLoading;
    }

    private IEnumerable<PageItem> VisiblePageItems
    {
        get
        {
            // Fixes paginator navigation by always keeping the first and last pages reachable.
            var visibleCount = Math.Max(5, MaxVisiblePages);
            if (SafeTotalPages <= visibleCount)
            {
                return Enumerable.Range(1, SafeTotalPages).Select(page => new PageItem(page, false));
            }

            var siblingCount = Math.Max(1, (visibleCount - 3) / 2);
            var pageNumbers = new SortedSet<int>
            {
                1,
                SafeTotalPages
            };

            for (var page = SafeCurrentPage - siblingCount; page <= SafeCurrentPage + siblingCount; page++)
            {
                if (page > 1 && page < SafeTotalPages)
                {
                    pageNumbers.Add(page);
                }
            }

            return BuildPageItems(pageNumbers);
        }
    }

    private string Summary
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(SummaryText))
            {
                return SummaryText;
            }

            return TotalItems.HasValue
                ? string.Format(
                    DashboardLocalizer["ItemsPageOf"].Value,
                    TotalItems.Value,
                    string.IsNullOrWhiteSpace(ItemsLabel) ? DashboardLocalizer["Items"].Value : ItemsLabel,
                    SafeCurrentPage,
                    SafeTotalPages
                )
                : string.Format(DashboardLocalizer["PageOf"].Value, SafeCurrentPage, SafeTotalPages);
        }
    }

    private string PageButtonClass(bool isActive)
    {
        return isActive
            ? "inline-flex h-10 min-w-10 items-center justify-center rounded-xl bg-slate-900 px-3 text-sm font-semibold text-white shadow-sm"
            : "inline-flex h-10 min-w-10 items-center justify-center rounded-xl border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 transition hover:border-slate-300 hover:text-slate-900";
    }

    private string NavigationButtonClass()
    {
        return "inline-flex h-10 min-w-10 items-center justify-center rounded-xl border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 transition hover:border-slate-300 hover:text-slate-900 disabled:cursor-not-allowed disabled:opacity-50";
    }

    private bool IsPageButtonDisabled(int page)
    {
        return IsLoading || page == SafeCurrentPage;
    }

    private bool IsControlLoading(string controlKey)
    {
        return IsLoading && _loadingControlKey == controlKey;
    }

    private static IEnumerable<PageItem> BuildPageItems(IEnumerable<int> pageNumbers)
    {
        int? previousPage = null;
        foreach (var page in pageNumbers)
        {
            if (previousPage.HasValue && page - previousPage.Value > 1)
            {
                yield return new PageItem(previousPage.Value + 1, true);
            }

            yield return new PageItem(page, false);
            previousPage = page;
        }
    }

    private static string PageControlKey(int page) => $"page:{page}";

    private Task GoToFirstPageAsync() => SetPageAsync(1, "first");

    private Task GoToPreviousPageAsync() => SetPageAsync(SafeCurrentPage - 1, "previous");

    private Task GoToNextPageAsync() => SetPageAsync(SafeCurrentPage + 1, "next");

    private Task GoToLastPageAsync() => SetPageAsync(SafeTotalPages, "last");

    private Task SetPageAsync(int page) => SetPageAsync(page, PageControlKey(page));

    private async Task SetPageAsync(int page, string controlKey)
    {
        var targetPage = Math.Min(Math.Max(1, page), SafeTotalPages);

        if (IsLoading || targetPage == SafeCurrentPage)
        {
            return;
        }

        _loadingControlKey = controlKey;
        await CurrentPageChanged.InvokeAsync(targetPage);
    }
}
