using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Modal;

public partial class AppModalBody : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool NoPadding { get; set; } = false;

    private string _bodyClass => NoPadding
        ? $"flex-1 overflow-y-auto {Class}".Trim()
        : $"flex-1 overflow-y-auto p-4 sm:p-5 {Class}".Trim();
}
