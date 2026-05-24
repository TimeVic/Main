using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.NotificationCenter;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.NotificationCenter.Parts;

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
        Dispatcher.Dispatch(new MarkAsReadAction(Notification.Id));
        MudDialog.Close();
        Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string.Format(SiteUrl.Dashboard_Task, _authState.Value.Workspace.Id, Notification.Task.TaskId));
    }
}
