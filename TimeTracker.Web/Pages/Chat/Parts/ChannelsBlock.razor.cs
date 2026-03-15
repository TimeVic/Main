using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Store.Messaging.Channels;

namespace TimeTracker.Web.Pages.Chat.Parts;

public partial class ChannelsBlock: IDisposable
{
    [Inject]
    protected IState<ChannelsState> ChannelsState { get; set; }
    
    [Inject] 
    private MessagingWebSocketClientService _webSocketClientService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _webSocketClientService.OnChannelCreated += OnChannelCreated;
        
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadListAction());
    }

    private void OnChannelCreated(MessagingChannelDto channel)
    {
        Dispatcher.Dispatch(new AddChannelAction(channel));
    }

    public void Dispose()
    {
        _webSocketClientService.OnChannelCreated -= OnChannelCreated;
    }
    
    private string GetNavButtonClass(bool isActive)
    {
        return isActive
            ? "flex w-full items-center justify-between rounded-2xl border border-cyan-200 bg-cyan-50 px-3 py-3 text-left transition"
            : "flex w-full items-center justify-between rounded-2xl border border-transparent bg-white px-3 py-3 text-left transition hover:border-slate-200 hover:bg-slate-50";
    }

    private void SelectChannel(MessagingChannelDto? channel)
    {
        Dispatcher.Dispatch(new SetSelectedAction(channel));
        Dispatcher.Dispatch(new Store.Messaging.Messages.LoadListAction());
    }
}
