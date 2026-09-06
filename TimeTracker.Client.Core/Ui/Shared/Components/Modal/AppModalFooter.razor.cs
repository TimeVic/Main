using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Modal;

public partial class AppModalFooter : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }
}
