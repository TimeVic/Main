using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Shared.Components.List;

public partial class TPaginator
{
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }
    
    [Parameter]
    public int TotalPages { get; set; } = 1;
    
    [Parameter]
    public string SummaryText { get; set; } = string.Empty;
    
    [Parameter]
    public int MaxVisiblePages { get; set; } = 5;

    private IEnumerable<int> VisiblePages
    {
        get
        {   
            var safeTotalPages = Math.Max(1, TotalPages);
            var safeCurrentPage = Math.Min(Math.Max(1, CurrentPage), safeTotalPages);
            var visibleCount = Math.Max(1, MaxVisiblePages);
            var half = visibleCount / 2;
            var start = Math.Max(1, safeCurrentPage - half);
            var end = Math.Min(safeTotalPages, start + visibleCount - 1);

            if (end - start + 1 < visibleCount)
            {
                start = Math.Max(1, end - visibleCount + 1);
            }

            return Enumerable.Range(start, end - start + 1);
        }
    }

    private string _summaryText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SummaryText))
            {
                return $"Page {CurrentPage} of {TotalPages}";
            }
            return SummaryText;
        }
    }

    private Task GoToPreviousPageAsync() => SetPageAsync(CurrentPage - 1);

    private Task GoToNextPageAsync() => SetPageAsync(CurrentPage + 1);

    private async Task SetPageAsync(int page)
    {
        var targetPage = Math.Min(Math.Max(1, page), Math.Max(1, TotalPages));

        if (targetPage == CurrentPage)
        {
            return;
        }

        await CurrentPageChanged.InvokeAsync(targetPage);
    }
}
