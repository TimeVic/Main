using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.NotificationCenter;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.NotificationCenter;

public partial class NotificationCenterModal: IDisposable
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public IState<NotificationCenterState> _state { get; set; }
    
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }

    private bool _isLoading = false;
    private bool _isHasMore = false;
    private ICollection<NotificationDto> _list = new List<NotificationDto>();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ActionSubscriber.SubscribeToAction<SetListAction>(this, action =>
        {
            _isHasMore = _state.Value.IsListHasMore;
            _list = _state.Value.List;
            StateHasChanged();
        });
        ActionSubscriber.SubscribeToAction<SetIsListLoadingAction>(this, action =>
        {
            _isLoading = action.IsLoading;
            StateHasChanged();
        });
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

    public void Dispose()
    {
        ActionSubscriber.UnsubscribeFromAllActions(this);
    }
}
