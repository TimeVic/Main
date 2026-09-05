using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Client.Core.Ui.Shared.Components;

public partial class AppCard : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool IsHoverable { get; set; }

    [Parameter]
    public bool Hoverable
    {
        get => IsHoverable;
        set => IsHoverable = value;
    }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    protected string ComputedClass
    {
        get
        {
            var baseClasses = "relative flex flex-col rounded-2xl border border-slate-200/80 dark:border-slate-700/60 bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-100 shadow-xs transition-all duration-200";
            var interactiveClass = (IsHoverable || OnClick.HasDelegate)
                ? "hover:shadow-md hover:border-slate-300 dark:hover:border-slate-600 cursor-pointer"
                : "";

            return $"{baseClasses} {interactiveClass} {Class}".Trim();
        }
    }
}
