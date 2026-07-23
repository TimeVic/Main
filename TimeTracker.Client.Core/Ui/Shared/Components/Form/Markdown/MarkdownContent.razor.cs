using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form;

public partial class MarkdownContent
{
    [Parameter]
    public string? Content { get; set; }

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public int MinHeight { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    private bool IsEmpty => string.IsNullOrWhiteSpace(Content);
    private MarkupString MarkdownHtml => (MarkupString) MarkdownService.ToHtml(Content ?? string.Empty);
    private string ContainerClass => $"markdown-content markdown-content-surface {Class}{(OnClick.HasDelegate ? " is-interactive" : string.Empty)}";
    private string? MinHeightStyle => MinHeight > 0 ? $"min-height: {MinHeight}px" : null;

    private Task OnContentClicked()
    {
        return OnClick.HasDelegate ? OnClick.InvokeAsync() : Task.CompletedTask;
    }
}
