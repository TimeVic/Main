using System.Timers;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.NotificationCenter;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.NotificationCenter;

public partial class NotificationBadgeButton: IDisposable
{
    [Parameter]
    public string Class { get; set; } = "";
    
    [Inject]
    public IState<NotificationCenterState> _state { get; set; }
    
    [Inject]
    public ModalDialogProviderService _modalDialogProviderService { get; set; }
    
    private System.Timers.Timer _timer;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _timer = new System.Timers.Timer();
        _timer.Interval = 30_000;
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
        Dispatcher.Dispatch(new LoadUnreadCountAction());
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Dispatch(new LoadUnreadCountAction());
    }
    
    public new void Dispose()
    {
        _timer.Dispose();
    }

    private Task ShowNotificationsModal()
    {
        return _modalDialogProviderService.ShowNotificationsCenterModal();
    }
}
