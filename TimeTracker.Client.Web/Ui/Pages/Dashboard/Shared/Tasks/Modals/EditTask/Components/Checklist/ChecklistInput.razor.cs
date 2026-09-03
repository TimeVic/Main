using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask.Components.Checklist;

public partial class ChecklistInput
{
    private ElementReference _inputRef;
    private string _title = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public EventCallback<string> OnAdd { get; set; }

    private void HandleInput(ChangeEventArgs e)
    {
        _title = e.Value?.ToString() ?? string.Empty;
    }

    private async System.Threading.Tasks.Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Submit();
        }
    }

    private async System.Threading.Tasks.Task Submit()
    {
        var trimmed = _title.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        _title = string.Empty;
        await OnAdd.InvokeAsync(trimmed);
    }
}
