using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Components.Core.Modal.Components;

public partial class AppModalBody : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool NoPadding { get; set; } = false;

    private string _bodyClass => NoPadding
        ? (Class ?? string.Empty)
        : $"p-4 sm:p-5 {Class}".Trim();
}
