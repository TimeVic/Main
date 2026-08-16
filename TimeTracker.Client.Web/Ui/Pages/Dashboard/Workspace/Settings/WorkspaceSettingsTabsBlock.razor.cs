using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Core.Services.Security;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings;

public partial class WorkspaceSettingsTabsBlock
{
    [Parameter]
    public required string ActiveSection { get; set; }

    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    private string GetTabClass(string section)
    {
        var stateClass = section == ActiveSection
            ? "border-b-2 border-blue-600 text-blue-700"
            : "border-b-2 border-transparent text-slate-600 hover:border-slate-300 hover:text-slate-900";

        return $"inline-flex shrink-0 items-center gap-2 px-3 py-3 text-sm font-medium transition-colors {stateClass}";
    }
}
