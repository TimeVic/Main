using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainMenu
{
    private record MenuItemModel(string Name, string Icon, string Url, bool IsDisabled = false);

    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    private List<MenuItemModel> _navItems = new()
    {
        new MenuItemModel("Time Entries", "fa-regular fa-clock", SiteUrl.Dashboard_TimeEntry),
        new MenuItemModel("Summary", "fa-regular fa-bar-chart", SiteUrl.Dashboard_Reports_Summary),
        new MenuItemModel("Tasks", "fa-regular fa-square-check", SiteUrl.Dashboard_Tasks_Main),
        new MenuItemModel("Payments", "fa-regular fa-credit-card", SiteUrl.Dashboard_Payments, false),
        new MenuItemModel("", "fa-solid fa-sliders", SiteUrl.Dashboard_Workspace_Settings, false),
    };

    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += (_, _) => StateHasChanged();
    }

    private void OnMenuItemSelected(string itemUrl)
    {
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
}
