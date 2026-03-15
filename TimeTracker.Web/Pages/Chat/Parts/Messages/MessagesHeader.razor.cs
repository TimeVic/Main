using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Messaging.Channels;
using TimeTracker.Web.Store.Messaging.Messages;

namespace TimeTracker.Web.Pages.Chat.Parts.Messages;

public partial class MessagesHeader
{
    [Inject]
    protected IState<MessagesState> MessagesState { get; set; }
    
    [Inject]
    protected IState<ChannelsState> ChannelsState { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
}
