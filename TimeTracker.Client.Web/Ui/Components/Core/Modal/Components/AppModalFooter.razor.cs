using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Components.Core.Modal.Components;

public partial class AppModalFooter : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }
}
