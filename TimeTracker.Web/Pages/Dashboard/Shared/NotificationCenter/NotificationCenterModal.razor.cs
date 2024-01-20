using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.NotificationCenter;

namespace TimeTracker.Web.Pages.Dashboard.Shared.NotificationCenter;

public partial class NotificationCenterModal
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public IState<NotificationCenterState> _state { get; set; }

    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private void LoadMore()
    {
        Dispatcher.Dispatch(new LoadListAction(false));
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }

    private void OnClickReadAll()
    {
        Dispatcher.Dispatch(new MarkAllAsReadAction());
        OnCloseModal();
    }

    private void OnClickToNotification(NotificationDto notification)
    {
        OnCloseModal();
        NavigationManager.NavigateTo(string.Format(SiteUrl.Dashboard_Task, _authState.Value.Workspace.Id, notification.Task.TaskId));
    }
}
