using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Permissions;

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

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    public IState<WorkspacePermissionsState> WorkspacePermissionsState { get; set; } = null!;
    
    private List<MenuItemModel> _navItems = new()
    {
        new MenuItemModel("Time Entries", "fa-regular fa-clock", SiteUrl.Dashboard_TimeEntry),
        new MenuItemModel("Summary", "fa-regular fa-bar-chart", SiteUrl.Dashboard_Reports_Summary),
        new MenuItemModel(
            "Money",
            "fa-solid fa-chart-pie",
            SiteUrl.Dashboard_WorkspaceMoney,
            null,
            WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new MenuItemModel("Tasks", "fa-regular fa-square-check", SiteUrl.Dashboard_Tasks_Main),
        new MenuItemModel(
            "Member Payments",
            "fa-regular fa-credit-card",
            SiteUrl.Dashboard_MemberPayments,
            "",
            WorkspacePermission.ReadMemberPayment
        ),
        new MenuItemModel(
            "Client Payments",
            "fa-solid fa-money-bill-transfer",
            SiteUrl.Dashboard_ClientPayments,
            "",
            WorkspacePermission.ReadClientPayment,
            WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new MenuItemModel(
            "Payments report",
            "fa-regular fa-credit-card",
            SiteUrl.Dashboard_Reports_MemberPayments,
            "Reports",
            WorkspacePermission.ReadMemberPayment
        ),
        new MenuItemModel("Time entries report", "fa-regular fa-clock", SiteUrl.Dashboard_Reports_TimeEntries, "Reports"),
        new MenuItemModel(
            "",
            "fa-solid fa-sliders",
            SiteUrl.Dashboard_Workspace_Settings,
            null,
            WorkspacePermission.UpdateWorkspaceSettings
        ),
    };

    private IEnumerable<MenuItemModel> AvailableNavItems => _navItems.Where(HasMenuItemAccess);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += OnLocationChanged;
        WorkspacePermissionsState.StateChanged += OnWorkspacePermissionsChanged;
    }

    private void OnMenuItemSelected(string itemUrl)
    {
        var item = _navItems.FirstOrDefault(i => i.Url == itemUrl);
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
