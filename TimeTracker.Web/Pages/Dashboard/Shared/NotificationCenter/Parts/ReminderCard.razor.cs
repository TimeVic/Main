using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Shared.NotificationCenter.Parts;

public partial class ReminderCard
{
    [Parameter]
    public NotificationDto Notification { get; set; }
    
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    private void OnClickToNotification()
    {
        MudDialog.Close();
        NavigationManager.NavigateTo(string.Format(SiteUrl.Dashboard_Task, _authState.Value.Workspace.Id, Notification.Task.TaskId));
    }
}
