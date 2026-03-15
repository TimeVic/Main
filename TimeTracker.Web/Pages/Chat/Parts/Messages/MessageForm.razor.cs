using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Messaging.Channels;
using TimeTracker.Web.Store.Messaging.Messages;

namespace TimeTracker.Web.Pages.Chat.Parts.Messages;

public partial class MessageForm
{
    [Inject]
    protected IState<MessagesState> State { get; set; }
    
    [Inject]
    protected IState<ChannelsState> ChannelsState { get; set; }
    
    private bool IsCanSend => !State.Value.IsMessageSending 
                              && !string.IsNullOrEmpty(_messageText)
                              && ChannelsState.Value.SelectedId != null;
    
    private string _messageText = string.Empty;

    private void SendMessage()
    {
        Dispatcher.Dispatch(new SendMessageAction(_messageText));
        _messageText = string.Empty;
    }

    private void SeedReply()
    {
        throw new NotImplementedException();
    }

    private void OnMessageChanged(ChangeEventArgs obj)
    {
        _messageText = obj.Value?.ToString() ?? string.Empty;
    }
}
