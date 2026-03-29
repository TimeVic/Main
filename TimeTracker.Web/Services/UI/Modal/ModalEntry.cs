using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Services.UI.Modal;

public class ModalEntry
{
    public RenderFragment Fragment { get; set; }
    public TaskCompletionSource<object?> Tcs { get; set; }

}
