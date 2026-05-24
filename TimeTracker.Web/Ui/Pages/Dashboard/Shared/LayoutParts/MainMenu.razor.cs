using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Services.Security;
using TimeTracker.Client.Core.Store.Permissions;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainMenu: IDisposable
{
    private record MenuItemModel(
        string Name,
        string Icon,
        string Url,
        string? GroupName = null,
        params WorkspacePermission[] RequiredPermissions
    );
    
    private string Text(string key) => DashboardLocalizer[key].Value;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    public IState<WorkspacePermissionsState> WorkspacePermissionsState { get; set; } = null!;
    
    private IReadOnlyCollection<MenuItemModel> NavItems => new List<MenuItemModel>
    {
        new(Text("TimeEntries"), "fa-regular fa-clock", SiteUrl.Dashboard_TimeEntry),
        new(Text("Summary"), "fa-regular fa-bar-chart", SiteUrl.Dashboard_Reports_Summary),
        new(
            Text("Money"),
            "fa-solid fa-chart-pie",
            SiteUrl.Dashboard_WorkspaceMoney,
            null,
            WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new(Text("Tasks"), "fa-regular fa-square-check", SiteUrl.Dashboard_Tasks_Main),
        new(Text("Notes"), "fa-regular fa-note-sticky", SiteUrl.Dashboard_Notes),
        new(
            Text("MemberPayments"),
            "fa-regular fa-credit-card",
            SiteUrl.Dashboard_MemberPayments,
            "",
            WorkspacePermission.ReadMemberPayment
        ),
        new(
            Text("ClientPayments"),
            "fa-solid fa-money-bill-transfer",
            SiteUrl.Dashboard_ClientPayments,
            "",
            WorkspacePermission.ReadClientPayment,
            WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new(
            Text("PaymentsReportTitle"),
            "fa-regular fa-credit-card",
            SiteUrl.Dashboard_Reports_MemberPayments,
            Text("Reports"),
            WorkspacePermission.ReadMemberPayment
        ),
        new(Text("TimeEntriesReportTitle"), "fa-regular fa-clock", SiteUrl.Dashboard_Reports_TimeEntries, Text("Reports")),
        new(
            string.Empty,
            "fa-solid fa-sliders",
            SiteUrl.Dashboard_Workspace_Settings,
            null,
            WorkspacePermission.UpdateWorkspaceSettings
        ),
    };

    private IEnumerable<MenuItemModel> AvailableNavItems => NavItems.Where(HasMenuItemAccess);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += OnLocationChanged;
        WorkspacePermissionsState.StateChanged += OnWorkspacePermissionsChanged;
    }

    private void OnMenuItemSelected(string itemUrl)
    {
        var item = NavItems.FirstOrDefault(i => i.Url == itemUrl);
        if (item != null && !HasMenuItemAccess(item))
        {
            return;
        }

        NavigationManager.NavigateTo(itemUrl);
    }
    
    private bool IsMenuItemSelected(MenuItemModel item)
    {
        var path = NavigationManager.GetPath();
        var basePath = SiteUrl.Dashboard_TimeEntry;
        if (item.Url != basePath)
        {
            // Also activate the workspace settings icon when the user settings page is open
            if (item.Url == SiteUrl.Dashboard_Workspace_Settings
                && path.StartsWith(SiteUrl.Dashboard_User_Settings))
            {
                return true;
            }
            return path.StartsWith(item.Url);
        }
        return item.Url == path;
    }

    private bool HasMenuItemAccess(MenuItemModel item)
    {
        return !WorkspacePermissionsState.Value.IsLoaded
            || item.RequiredPermissions.Length == 0
            || item.RequiredPermissions.All(SecurityManager.HasPermission);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        StateHasChanged();
    }

    private void OnWorkspacePermissionsChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        WorkspacePermissionsState.StateChanged -= OnWorkspacePermissionsChanged;
    }
}
