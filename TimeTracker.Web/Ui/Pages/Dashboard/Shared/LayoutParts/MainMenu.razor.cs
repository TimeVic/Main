using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainMenu
{
    private record MenuItemModel(
        string Name,
        string Icon,
        string Url,
        string? GroupName = null,
        WorkspacePermission? RequiredPermission = null
    );

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;
    
    private List<MenuItemModel> _navItems = new()
    {
        new MenuItemModel("Time Entries", "fa-regular fa-clock", SiteUrl.Dashboard_TimeEntry),
        new MenuItemModel("Summary", "fa-regular fa-bar-chart", SiteUrl.Dashboard_Reports_Summary),
        new MenuItemModel(
            "Money",
            "fa-solid fa-chart-pie",
            SiteUrl.Dashboard_WorkspaceMoney,
            RequiredPermission: WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new MenuItemModel("Tasks", "fa-regular fa-square-check", SiteUrl.Dashboard_Tasks_Main),
        new MenuItemModel("Member Payments", "fa-regular fa-credit-card", SiteUrl.Dashboard_MemberPayments, ""),
        new MenuItemModel(
            "Client Payments",
            "fa-solid fa-money-bill-transfer",
            SiteUrl.Dashboard_ClientPayments,
            "",
            WorkspacePermission.ReadWorkspaceFinancialSummary
        ),
        new MenuItemModel("Payments report", "fa-regular fa-credit-card", SiteUrl.Dashboard_Reports_MemberPayments, "Reports"),
        new MenuItemModel("Time entries report", "fa-regular fa-clock", SiteUrl.Dashboard_Reports_TimeEntries, "Reports"),
        new MenuItemModel(
            "",
            "fa-solid fa-sliders",
            SiteUrl.Dashboard_Workspace_Settings,
            RequiredPermission: WorkspacePermission.UpdateWorkspaceSettings
        ),
    };

    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += (_, _) => StateHasChanged();
    }

    private void OnMenuItemSelected(string itemUrl)
    {
        var item = _navItems.FirstOrDefault(i => i.Url == itemUrl);
        if (item != null && IsMenuItemDisabled(item))
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

    private bool IsMenuItemDisabled(MenuItemModel item)
    {
        return item.RequiredPermission != null
            && !SecurityManager.HasPermission(item.RequiredPermission.Value);
    }
}
