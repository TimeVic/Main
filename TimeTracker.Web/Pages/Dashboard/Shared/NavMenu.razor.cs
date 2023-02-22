using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Pages.Dashboard.Shared;

public partial class NavMenu
{
    [Inject]
    protected IState<ClientState> ClientState { get; set; }
    
    [Inject]
    protected NavigationManager _navigationManager { get; set; }

    public ICollection<ClientDto> Clients => ClientState.Value.SortedList;

    public string WithoutClientTasksListUrl
    {
        get
        {
            return string.Format(SiteUrl.Dashboard_Tasks, "");
        }
    }

    private string GetTasksListUrl(long? clientId = null)
    {
        return string.Format(SiteUrl.Dashboard_Tasks, clientId?.ToString() ?? "0", "");
    }
}
