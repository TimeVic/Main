using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Permissions;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainMenu: IDisposable
{
    private record MenuItemModel(
        string Name,
        string Icon,
        string Url,
        string? GroupName = null,
        WorkspacePermission[]? RequiredPermissions = null,
        Func<bool>? IsVisible = null
    );
    
    private string Text(string key) => DashboardLocalizer[key].Value;

    [Inject]
    public ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    public IState<WorkspacePermissionsState> WorkspacePermissionsState { get; set; } = null!;

    [Parameter]
    public bool IsMobile { get; set; }
    
    private IReadOnlyCollection<MenuItemModel> NavItems => new List<MenuItemModel>
    {
        new(Text("Summary"), "fa-regular fa-bar-chart", UrlService.GetDashboardUrl("report/summary")),
        new(Text("TimeEntries"), "fa-regular fa-clock", UrlService.GetDashboardUrl()),
        new(Text("Tasks"), "fa-regular fa-square-check", UrlService.GetDashboardUrl("tasks")),
        new(Text("Notes"), "fa-regular fa-note-sticky", UrlService.GetDashboardUrl("notes")),
        new(
            Text("MemberPayments"),
            "fa-regular fa-credit-card",
            UrlService.GetDashboardUrl("member-payments"),
            Text("Operations"),
            [WorkspacePermission.ReadMemberPayment],
            () => IsTeamAdministrator
        ),
        new(
            Text("ClientPayments"),
            "fa-solid fa-money-bill-transfer",
            UrlService.GetDashboardUrl("client-payments"),
            IsTeamAdministrator ? Text("Operations") : string.Empty,
            [WorkspacePermission.ReadClientPayment],
            () => IsSoloWorkspace || IsTeamAdministrator
        ),
        new(
            Text("WorkspaceMoney"),
            "fa-solid fa-chart-pie",
            UrlService.GetDashboardUrl("money"),
            Text("Reports"),
            [WorkspacePermission.ReadWorkspaceFinancialSummary],
            () => IsTeamAdministrator
        ),
        new(
            Text("UserPaymentReport_Menu"),
            "fa-solid fa-wallet",
            UrlService.GetDashboardUrl("report/user-payments"),
            Text("Reports"),
            [WorkspacePermission.ReadUserPaymentReport]
        ),
        new(
            Text("TimeEntriesReportTitle"),
            "fa-regular fa-clock",
            UrlService.GetDashboardUrl("report/time-entries"),
            Text("Reports")
        ),
        new(
            string.Empty,
            "fa-solid fa-sliders",
            UrlService.GetDashboardUrl("workspace/settings"),
            null,
            [WorkspacePermission.UpdateWorkspaceSettings]
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
        var basePath = UrlService.GetDashboardUrl();
        if (item.Url != basePath)
        {
            // Also activate the workspace settings icon when the user settings page is open
            if (item.Url == UrlService.GetDashboardUrl("workspace/settings")
                && path.StartsWith(UrlService.GetDashboardUrl("user/settings")))
            {
                return true;
            }
            return path.StartsWith(item.Url);
        }
        return item.Url == path;
    }

    private string GetMenuLinkClass(MenuItemModel item)
    {
        var stateClass = IsMenuItemSelected(item)
            ? "bg-blue-50 text-blue-700"
            : "text-slate-600 hover:bg-slate-100 hover:text-slate-900";

        return $"inline-flex h-10 shrink-0 items-center gap-2 rounded-lg px-3 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-blue-200 {stateClass}";
    }

    private string GetMobileMenuLinkClass(MenuItemModel item)
    {
        var stateClass = IsMenuItemSelected(item)
            ? "bg-blue-50 text-blue-700"
            : "text-slate-700 hover:bg-slate-100";

        return $"flex w-full items-center gap-3 rounded-lg px-3 py-3 text-base font-medium transition-colors {stateClass}";
    }

    private bool HasMenuItemAccess(MenuItemModel item)
    {
        if (item.IsVisible?.Invoke() == false)
        {
            return false;
        }

        return !WorkspacePermissionsState.Value.IsLoaded
            || item.RequiredPermissions is not { Length: > 0 }
            || item.RequiredPermissions.All(SecurityManager.HasPermission);
    }

    private bool IsSoloWorkspace => AuthState.Value.Workspace?.Mode == WorkspaceMode.Solo;

    private bool IsTeamAdministrator => AuthState.Value.Workspace?.Mode == WorkspaceMode.Team
        && AuthState.Value.IsRoleAdmin;

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
