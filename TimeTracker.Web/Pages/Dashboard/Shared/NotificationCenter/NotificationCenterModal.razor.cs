using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Web.Store.NotificationCenter;

namespace TimeTracker.Web.Pages.Dashboard.Shared.NotificationCenter;

public partial class NotificationCenterModal
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public IState<NotificationCenterState> _state { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private void LoadMore()
    {
        Dispatcher.Dispatch(new LoadListAction(true));
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }

    private Task OnClickReadAll()
    {
        throw new NotImplementedException();
    }
}
