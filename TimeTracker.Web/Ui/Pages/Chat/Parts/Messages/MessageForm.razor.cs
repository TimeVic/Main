using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Web.Store.Messaging.Channels;
using TimeTracker.Web.Store.Messaging.Messages;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Messages;

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
        if (!string.IsNullOrWhiteSpace(_messageText))
        {
            Dispatcher.Dispatch(new SendMessageAction(_messageText));
            _messageText = string.Empty;
        }
    }

    private void SeedReply()
    {
        throw new NotImplementedException();
    }

    private void OnMessageChanged(ChangeEventArgs obj)
    {
        _messageText = obj.Value?.ToString() ?? string.Empty;
    }

    private async Task OnKeyDown(KeyboardEventArgs arg)
    {
        if (arg.Key == "Enter")
        {
            SendMessage();
        }
        await Task.CompletedTask;
    }
}
