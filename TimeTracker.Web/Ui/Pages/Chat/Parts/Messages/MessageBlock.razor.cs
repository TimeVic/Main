using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts.Messages;

public partial class MessageBlock
{
    [Parameter]
    public MessagingMessageDto Message { get; set; }
    
    [Parameter]
    public DateTime? PreviousMessageDay { get; set; }
    
    protected override bool ShouldRender()
    {
        // Message block should be re-rendered only if message day is different from previous message day
        return false;
    }

    
    private string GetMessageDayLabel(DateTime sentAt)
    {
        var messageDay = sentAt.Date;

        if (messageDay == DateTime.Today)
        {
            return "Today";
        }

        if (messageDay == DateTime.Today.AddDays(-1))
        {
            return "Yesterday";
        }
        return sentAt.ToString("dd MMM yyyy");
    }
    
    private string GetFormattedDate(DateTime dt)
    {
        if (dt.Date == DateTime.Today)
        {
            return Message.CreatedAt.ToString("HH:mm");
        }
        return dt.ToString("dd MMM yyyy HH:mm");
    }
}
