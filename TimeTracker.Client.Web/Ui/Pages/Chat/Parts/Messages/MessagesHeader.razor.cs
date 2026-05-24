using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Messaging.Channels;
using TimeTracker.Client.Core.Store.Messaging.Messages;

namespace TimeTracker.Client.Web.Ui.Pages.Chat.Parts.Messages;

public partial class MessagesHeader
{
    [Inject]
    protected IState<MessagesState> MessagesState { get; set; }
    
    [Inject]
    protected IState<ChannelsState> ChannelsState { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    private MessagesListState? ListState => MessagesState.Value.GetListState(ChannelsState.Value.SelectedChannel!);
}
